using System;
using System.Collections.Generic;
using System.Linq;

namespace MKtest.Checklists
{
    /// <summary>Determines where a new check belongs in its category and algorithm.</summary>
    public static class ChecklistCatalogPlacement
    {
        public static bool SameGroup(ChecklistCase left, ChecklistCase right) =>
            string.Equals(left.Category.Trim(), right.Category.Trim(),
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(left.Algorithm.Trim(), right.Algorithm.Trim(),
                StringComparison.OrdinalIgnoreCase);

        public static int InsertIndex(IReadOnlyList<ChecklistCase> catalog,
            ChecklistCase item, string? afterId = null)
        {
            if (!string.IsNullOrWhiteSpace(afterId))
            {
                for (int i = 0; i < catalog.Count; i++)
                {
                    if (catalog[i].Id.Equals(afterId, StringComparison.OrdinalIgnoreCase)
                        && SameGroup(catalog[i], item))
                        return i + 1;
                }
                throw new ArgumentException(
                    "Выбранная проверка отсутствует в указанном разделе или алгоритме.",
                    nameof(afterId));
            }

            // Default: append to the end of this section/algorithm, preserving
            // the order of every existing check (and its printed number).
            for (int i = catalog.Count - 1; i >= 0; i--)
                if (SameGroup(catalog[i], item))
                    return i + 1;
            return catalog.Count;
        }

        public static IEnumerable<ChecklistCase> GroupMembers(
            IReadOnlyList<ChecklistCase> catalog, ChecklistCase item) =>
            catalog.Where(x => SameGroup(x, item));
    }
}
