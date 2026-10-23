using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Checklists;

namespace MKtest.Tests;

[TestClass]
public sealed class ChecklistPrintLayoutTests
{
    [TestMethod]
    public void PortraitList_UsesOnlyFourRequestedColumns()
    {
        CollectionAssert.AreEqual(new[]
        {
            "№", "Раздел / алгоритм", "Проверка", "Результат / замечания"
        }, ChecklistPrintLayout.ListColumns);

        Assert.AreEqual(4, ChecklistPrintLayout.ListColumnRatios.Length);
        Assert.IsTrue(Math.Abs(ChecklistPrintLayout.ListColumnRatios.Sum() - 1f) < .0001f);
        Assert.IsFalse(ChecklistPrintLayout.ListColumns.Any(x =>
            x is "ID" or "ОС" or "Ожидается" or "Галочки"));
    }

    [TestMethod]
    public void OperatingSystems_ShowsAllDistinctPlatformsInHeader()
    {
        var cases = new[]
        {
            new ChecklistCase { Platforms = "Android 11, Linux" },
            new ChecklistCase { Platforms = "Linux, Android 7" },
            new ChecklistCase { Platforms = "Android 11" }
        };

        Assert.AreEqual("Android 11, Android 7, Linux",
            ChecklistPrintLayout.OperatingSystems(cases));
    }

    [TestMethod]
    public void OperatingSystems_MissingPlatform_IsNotInvented()
    {
        Assert.AreEqual("Не указана",
            ChecklistPrintLayout.OperatingSystems(new[] { new ChecklistCase() }));
    }

    [TestMethod]
    public void PrintedNumber_IsStableRunPositionWithoutInternalId()
    {
        var second = new ChecklistCase
        {
            Id = "AND-047 / LIN-048", Action = "Проверка статистики"
        };
        var document = new ChecklistDocument
        {
            Cases = new List<ChecklistCase>
            {
                new() { Id = "FARE-011" }, second,
                new() { Id = "USER-003" }
            }
        };
        Assert.AreEqual(2, ChecklistPrintLayout.NumberInRun(document, second));
        Assert.AreEqual(3, ChecklistPrintLayout.NumberInRun(document, document.Cases[2]));
        Assert.ThrowsExactly<ArgumentException>(() =>
            ChecklistPrintLayout.NumberInRun(document,
                new ChecklistCase { Id = "AND-047 / LIN-048" }));
    }

    [TestMethod]
    public void PrintSection_KeepsFareAlgorithmButNotOsColumn()
    {
        var check = new ChecklistCase
        {
            Category = "Безбилетники", Algorithm = "СПб ГКУ",
            Platforms = "Android 11", Module = "Статистика",
            Action = "Проверить счётчик"
        };
        Assert.AreEqual("Безбилетники\nСПб ГКУ",
            ChecklistPrintLayout.Section(check));
        Assert.AreEqual("Статистика\nПроверить счётчик",
            ChecklistPrintLayout.Inspection(check));
    }
}
