using System;
using System.Collections.Generic;
using System.Linq;

namespace MKtest.Checklists
{
    /// <summary>
    /// Unifies old fare statistics and passenger notifications into one category,
    /// without discarding their original ID or changing their test requirements.
    /// </summary>
    public static class ChecklistCatalogRules
    {
        public const string FareCategory = "Безбилетники";
        public const string CityAlgorithm = "СПб ГКУ";
        public const string RegionalAlgorithm = "Региональный";

        public static void Normalize(ChecklistCase item)
        {
            ArgumentNullException.ThrowIfNull(item);
            string category = item.Category.Trim();
            if (category.Equals("Статистика не оплативших", StringComparison.OrdinalIgnoreCase))
            {
                item.Category = FareCategory;
                item.Algorithm = CityAlgorithm;
            }
            else if (category.Equals(FareCategory, StringComparison.OrdinalIgnoreCase))
            {
                item.Category = FareCategory;
                if (string.IsNullOrWhiteSpace(item.Algorithm))
                    item.Algorithm = RegionalAlgorithm;
            }
            else
            {
                // A classification is only meaningful within the fare category.
                item.Algorithm = "";
            }
        }

        public static List<ChecklistCase> NormalizeCopy(IEnumerable<ChecklistCase> source)
        {
            var copy = source.Select(c => c.NewBlankResult()).ToList();
            foreach (ChecklistCase item in copy)
                Normalize(item);
            return copy;
        }

        public static bool MatchesFareAlgorithm(ChecklistCase item, string algorithm)
        {
            if (item.Category != FareCategory) return false;
            return algorithm == "Оба алгоритма" ||
                item.Algorithm.Equals(algorithm, StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasDuplicateId(IEnumerable<ChecklistCase> items, string id,
            ChecklistCase? except = null) =>
            items.Any(x => !ReferenceEquals(x, except) &&
                x.Id.Trim().Equals(id.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
