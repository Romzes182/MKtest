using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Checklists;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace MKtest.Tests;

[TestClass]
public sealed class ChecklistExcelImporterTests
{
    private const string SheetNamespace =
        "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string Relationships =
        "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    [TestMethod]
    public void ImportCombinedWorkbook_RetainsModuleAndExpectedResult()
    {
        string file = CreateWorkbook("Общий список", 6,
            ["ID", "Раздел", "Платформа / схема", "Сценарий / проверка",
             "Ожидаемый результат"],
            ["FARE-004", "Безбилетники",
             "Linux, Android 7, Android 11\nИР-0652 / Json RPC",
             "Количество безбилетных пассажиров = 3",
             "Через 30 секунд звучит оповещение"], "combined.xlsx");
        try
        {
            var items = ChecklistExcelImporter.Import([file]);
            Assert.HasCount(1, items);
            Assert.AreEqual("FARE-004", items[0].Id);
            Assert.AreEqual("Linux, Android 7, Android 11", items[0].Platforms);
            Assert.AreEqual("ИР-0652 / Json RPC", items[0].Module);
            Assert.AreEqual("Через 30 секунд звучит оповещение", items[0].Expected);
            Assert.AreEqual("Не проверено", items[0].Result);
        }
        finally { File.Delete(file); }
    }

    [TestMethod]
    public void ImportAndroidAndLinux_MergesOnlyEqualTestsAndPreservesBothPlatforms()
    {
        string android = CreateWorkbook("Чек-лист", 32,
            ["№", "Категория", "Описание", "Статус"],
            ["1", "Дата и время", "Отображение текущей даты", "Есть баг"],
            "report Android.xlsx");
        string linux = CreateWorkbook("Чек-лист", 33,
            ["№", "Категория", "Описание", "Статус"],
            ["1", "Дата и время", "Отображение текущей даты", "Выполнен"],
            "report linux.xlsx");
        try
        {
            var items = ChecklistExcelImporter.Import([android, linux]);
            Assert.HasCount(1, items);
            Assert.AreEqual("AND-001 / LIN-001", items[0].Id);
            StringAssert.Contains(items[0].Platforms, "Android 11");
            StringAssert.Contains(items[0].Platforms, "Linux");
            Assert.AreEqual("Не проверено", items[0].Result);
        }
        finally { File.Delete(android); File.Delete(linux); }
    }

    [TestMethod]
    public void ImportEmergency_SavesActionsAndExpectedResultWithoutLegacyStatus()
    {
        string emergency = CreateWorkbook("Чек-лист", 27,
            ["ID", "Проверка", "Ожидаемый результат", "Статус", "Баг"],
            ["Т2.1", "Запустить видео-файл ком.5",
             "Видео запускается автоматически", "Есть баг", "Б2"],
            "отчет МЧС.xlsx");
        try
        {
            var items = ChecklistExcelImporter.Import([emergency]);
            Assert.HasCount(1, items);
            Assert.AreEqual("MCH-Т2.1", items[0].Id);
            Assert.AreEqual("Видео запускается автоматически", items[0].Expected);
            Assert.AreEqual("", items[0].Bug);
        }
        finally { File.Delete(emergency); }
    }

    private static string CreateWorkbook(
        string sheetName, int headerRow, string[] headers,
        string[] data, string filename)
    {
        string directory = Path.Combine(Path.GetTempPath(),
            "MKtest-checklist-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, filename);

        using (var zip = ZipFile.Open(path, ZipArchiveMode.Create))
        {
            Write(zip, "xl/workbook.xml",
                $"<workbook xmlns=\"{SheetNamespace}\" xmlns:r=\"{Relationships}\">" +
                $"<sheets><sheet name=\"{sheetName}\" sheetId=\"1\" r:id=\"r1\"/></sheets></workbook>");
            Write(zip, "xl/_rels/workbook.xml.rels",
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"r1\" Target=\"worksheets/sheet1.xml\" /></Relationships>");
            string Row(int number, string[] cells) =>
                $"<row r=\"{number}\">" +
                string.Concat(cells.Select((value, index) =>
                    $"<c r=\"{(char)('A' + index)}{number}\" t=\"inlineStr\">" +
                    $"<is><t>{SecurityElement.Escape(value)}</t></is></c>")) +
                "</row>";
            Write(zip, "xl/worksheets/sheet1.xml",
                $"<worksheet xmlns=\"{SheetNamespace}\"><sheetData>" +
                Row(headerRow, headers) + Row(headerRow + 1, data) +
                "</sheetData></worksheet>");
        }

        return path;
    }

    private static void Write(ZipArchive zip, string entry, string contents)
    {
        using var writer = new StreamWriter(zip.CreateEntry(entry).Open(),
            new UTF8Encoding(false));
        writer.Write(contents);
    }
}
