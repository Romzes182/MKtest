using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MKtest.Checklists
{
    /// <summary>
    /// Imports .xlsx with the .NET built-in ZIP/XML APIs, without Excel or
    /// external libraries. Handles the four original supplied checklist formats
    /// and the combined workbook with headers from the print template.
    /// </summary>
    public static class ChecklistExcelImporter
    {
        private static readonly XNamespace Ns =
            "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace Rel =
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        public static List<ChecklistCase> Import(IEnumerable<string> paths)
        {
            var items = new List<ChecklistCase>();
            foreach (string path in paths)
                items.AddRange(ReadFile(path));

            // Merge duplicate basic checks shared by Android and Linux, but
            // preserve distinct payment/announcer combinations and MChS cases.
            var dedup = new Dictionary<string, ChecklistCase>(StringComparer.OrdinalIgnoreCase);
            foreach (ChecklistCase item in items)
            {
                string key = item.Category.Trim() + "\u001f" +
                    item.Module.Trim() + "\u001f" + item.Action.Trim();
                if (dedup.TryGetValue(key, out ChecklistCase? existing))
                {
                    if (existing.Section is "Android" or "Linux" &&
                        item.Section is "Android" or "Linux")
                    {
                        if (!existing.Platforms.Split(',')
                            .Any(p => p.Trim().Equals(item.Platforms,
                                StringComparison.OrdinalIgnoreCase)))
                            existing.Platforms += ", " + item.Platforms;
                        existing.Id += " / " + item.Id;
                        continue;
                    }
                    key += "\u001f" + item.Id;
                }

                dedup[key] = item;
            }

            return dedup.Values.OrderBy(x => x.Category)
                .ThenBy(x => x.Id).ToList();
        }

        private static IEnumerable<ChecklistCase> ReadFile(string path)
        {
            using var archive = ZipFile.OpenRead(path);
            var workbook = ReadXml(archive, "xl/workbook.xml");
            var relations = ReadXml(archive, "xl/_rels/workbook.xml.rels");
            var relationship = relations.Descendants()
                .Where(x => x.Name.LocalName == "Relationship")
                .ToDictionary(x => (string?)x.Attribute("Id") ?? "",
                              x => (string?)x.Attribute("Target") ?? "");

            XElement sheet = workbook.Descendants(Ns + "sheet")
                .FirstOrDefault(x => ((string?)x.Attribute("name")) == "Общий список")
                ?? workbook.Descendants(Ns + "sheet")
                    .First(x => ((string?)x.Attribute("name")) == "Чек-лист");

            string relId = (string?)sheet.Attribute(Rel + "id") ?? "";
            if (!relationship.TryGetValue(relId, out string? target))
                throw new InvalidDataException("Нет таблицы чек-листа в файле " + path);

            string sheetPath = target!.TrimStart('/').Replace('\\', '/');
            if (!sheetPath.StartsWith("xl/", StringComparison.OrdinalIgnoreCase))
                sheetPath = "xl/" + sheetPath;

            List<string> shared = new();
            ZipArchiveEntry? sharedEntry = archive.GetEntry("xl/sharedStrings.xml");
            if (sharedEntry != null)
            {
                using var stream = sharedEntry.Open();
                var sharedXml = XDocument.Load(stream);
                shared = sharedXml.Descendants(Ns + "si")
                    .Select(x => string.Concat(x.Descendants(Ns + "t").Select(t => t.Value)))
                    .ToList();
            }

            XDocument data = ReadXml(archive, sheetPath);
            var rows = data.Descendants(Ns + "row")
                .Select(row => new
                {
                    Index = (int?)row.Attribute("r") ?? 0,
                    Cells = row.Elements(Ns + "c").ToDictionary(
                        c => Regex.Match((string?)c.Attribute("r") ?? "", "^[A-Z]+").Value,
                        c => CellValue(c, shared))
                })
                .ToList();

            string Read(Dictionary<string, string> cells, string column) =>
                cells.TryGetValue(column, out string? value) ? value.Trim() : "";
            string filename = Path.GetFileName(path).ToLowerInvariant();
            string section = filename.Contains("безбилет") ? "Безбилетники" :
                filename.Contains("мчс") ? "МЧС" :
                filename.Contains("linux") ? "Linux" : "Android";
            bool combined = ((string?)sheet.Attribute("name")) == "Общий список";

            var header = rows.FirstOrDefault(r =>
                r.Cells.Values.Any(v => v == "Категория" || v == "Проверка" ||
                    v == "Сценарий" || v == "Сценарий / проверка"));
            if (header == null)
                throw new InvalidDataException("Заголовок таблицы не найден: " + path);

            foreach (var row in rows.Where(x => x.Index > header.Index))
            {
                string id = Read(row.Cells, "A");
                if (string.IsNullOrWhiteSpace(id)) continue;
                var item = new ChecklistCase
                {
                    Id = combined ? id :
                        section == "МЧС" ? "MCH-" + id :
                        (section == "Linux" ? "LIN-" :
                         section == "Безбилетники" ? "FARE-" : "AND-") +
                        (int.TryParse(id, out int serial)
                            ? serial.ToString("000") : id),
                    Section = section
                };
                if (combined)
                {
                    item.Category = Read(row.Cells, "B");
                    string platformAndModule = Read(row.Cells, "C").Replace("\r", "");
                    string[] split = platformAndModule.Split('\n', 2);
                    item.Platforms = split[0];
                    item.Module = split.Length > 1 ? split[1] : "";
                    item.Action = Read(row.Cells, "D");
                    item.Expected = Read(row.Cells, "E");
                    item.Section = "Общий";
                }
                else if (section == "Безбилетники")
                {
                    item.Module = Read(row.Cells, "B") + " / " + Read(row.Cells, "C");
                    item.Category = "Безбилетники";
                    item.Platforms = "Linux, Android 7, Android 11";
                    item.Action = Read(row.Cells, "D");
                    item.Expected = Read(row.Cells, "E");
                }
                else if (section == "МЧС")
                {
                    item.Category = "МЧС";
                    item.Platforms = "Android 7, Android 11";
                    item.Module = "Команды МЧС";
                    item.Action = Read(row.Cells, "B");
                    item.Expected = Read(row.Cells, "C");
                }
                else
                {
                    item.Category = Read(row.Cells, "B");
                    item.Platforms = section == "Android" ? "Android 11" : "Linux";
                    item.Action = Read(row.Cells, "C");
                }
                if (string.IsNullOrWhiteSpace(item.Action))
                    continue;
                if (!combined && section != "МЧС" &&
                    !int.TryParse(id, out _))
                    continue;
                yield return item;
            }
        }

        private static XDocument ReadXml(ZipArchive archive, string path)
        {
            ZipArchiveEntry? entry = archive.GetEntry(path);
            if (entry == null) throw new InvalidDataException("Не найден лист: " + path);
            using var stream = entry.Open();
            return XDocument.Load(stream);
        }

        private static string CellValue(XElement cell, IReadOnlyList<string> shared)
        {
            string type = (string?)cell.Attribute("t") ?? "";
            if (type == "inlineStr")
                return string.Concat(cell.Descendants(Ns + "t").Select(x => x.Value));
            string value = cell.Element(Ns + "v")?.Value ?? "";
            if (type == "s" && int.TryParse(value, out int ix) &&
                ix >= 0 && ix < shared.Count)
                return shared[ix];
            return value;
        }
    }
}
