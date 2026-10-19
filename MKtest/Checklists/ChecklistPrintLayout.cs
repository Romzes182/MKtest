using System;
using System.Collections.Generic;
using System.Linq;

namespace MKtest.Checklists
{
    /// <summary>
    /// Shared paper-template rules for the portrait A4 list and single test.
    /// Printed numbers are positions in the saved run, not internal catalog IDs.
    /// </summary>
    public static class ChecklistPrintLayout
    {
        public static readonly string[] ListColumns =
        {
            "№", "Раздел / алгоритм", "Проверка", "Результат / замечания"
        };

        public static readonly float[] ListColumnRatios =
        {
            0.075f, 0.205f, 0.48f, 0.24f
        };

        public static int NumberInRun(ChecklistDocument document, ChecklistCase item)
        {
            int index = document.Cases.IndexOf(item);
            if (index < 0)
                throw new ArgumentException(
                    "Проверка отсутствует в текущем обходе.", nameof(item));
            return index + 1;
        }

        public static string OperatingSystems(IEnumerable<ChecklistCase> items)
        {
            string[] values = items
                .SelectMany(x => (x.Platforms ?? "").Split(',',
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return values.Length == 0 ? "Не указана" : string.Join(", ", values);
        }

        public static string RunOperatingSystem(
            ChecklistDocument document, IEnumerable<ChecklistCase> cases)
        {
            if (!string.IsNullOrWhiteSpace(document.RunPlatform))
                return document.RunPlatform.Trim();

            // Migration of existing runs: the selected environment used to be
            // stored in Environment; prefer it over per-check platform lists.
            string legacy = (document.Environment ?? "").Trim();
            if (legacy.Equals("Android 7", StringComparison.OrdinalIgnoreCase) ||
                legacy.Equals("Android 11", StringComparison.OrdinalIgnoreCase) ||
                legacy.Equals("Linux", StringComparison.OrdinalIgnoreCase) ||
                legacy.Equals("Android (все)", StringComparison.OrdinalIgnoreCase))
                return legacy;

            return OperatingSystems(cases);
        }

        public static string HeaderEnvironment(
            ChecklistDocument document, IEnumerable<ChecklistCase> cases)
        {
            string platform = RunOperatingSystem(document, cases);
            string details = document.PrintHeader?.EnvironmentDetails?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(details)) return platform;
            return platform == "Не указана"
                ? details : platform + ", " + details;
        }

        public static string Section(ChecklistCase item) =>
            string.IsNullOrWhiteSpace(item.Algorithm)
                ? item.Category
                : item.Category + "\n" + item.Algorithm;

        public static string Inspection(ChecklistCase item) =>
            string.IsNullOrWhiteSpace(item.Module)
                ? item.Action
                : item.Module + "\n" + item.Action;
    }
}
