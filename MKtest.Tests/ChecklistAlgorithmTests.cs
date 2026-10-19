using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Checklists;

namespace MKtest.Tests;

[TestClass]
public sealed class ChecklistAlgorithmTests
{
    [TestMethod]
    public void BuiltInCatalog_MergesCategoriesWithoutLosingOriginalChecks()
    {
        var cases = BuiltInChecklistCatalog.Load();

        Assert.AreEqual(246, cases.Count, "The existing master catalog must retain all checks.");
        Assert.IsFalse(cases.Any(x => x.Category == "Статистика не оплативших"));
        var fare = cases.Where(x => x.Category == ChecklistCatalogRules.FareCategory).ToList();
        Assert.AreEqual(104, fare.Count);
        Assert.AreEqual(8, fare.Count(x =>
            x.Algorithm == ChecklistCatalogRules.CityAlgorithm));
        Assert.AreEqual(96, fare.Count(x =>
            x.Algorithm == ChecklistCatalogRules.RegionalAlgorithm));
        Assert.IsTrue(fare.Any(x => x.Id == "AND-047 / LIN-048" &&
            x.Algorithm == ChecklistCatalogRules.CityAlgorithm));
        Assert.IsTrue(fare.Any(x => x.Id == "FARE-016" &&
            x.Algorithm == ChecklistCatalogRules.RegionalAlgorithm));
    }

    [TestMethod]
    public void LegacyCatalog_MigratesCategoryButPreservesResultsAndReferences()
    {
        var stat = new ChecklistCase
        {
            Id = "LIN-050", Category = "Статистика не оплативших",
            Action = "Проверить подсчёт", Result = "Баг",
            Bug = "ERR-471", Notes = "Повторяется после перезапуска"
        };
        var fare = new ChecklistCase
        {
            Id = "FARE-004", Category = "Безбилетники",
            Action = "Проверить оповещение"
        };
        ChecklistCatalogRules.Normalize(stat);
        ChecklistCatalogRules.Normalize(fare);

        Assert.AreEqual("Безбилетники", stat.Category);
        Assert.AreEqual("СПб ГКУ", stat.Algorithm);
        Assert.AreEqual("Баг", stat.Result);
        Assert.AreEqual("ERR-471", stat.Bug);
        Assert.AreEqual("Повторяется после перезапуска", stat.Notes);
        Assert.AreEqual("Региональный", fare.Algorithm);
    }

    [TestMethod]
    public void CatalogEditorDraft_DoesNotAlterSavedRunsAndPreservesAlgorithmOnClone()
    {
        var original = new ChecklistCase
        {
            Id = "FARE-011", Category = "Безбилетники",
            Algorithm = "Региональный", Action = "Проверка",
            Result = "Есть"
        };
        var draft = ChecklistCatalogRules.NormalizeCopy(new[] { original });
        draft[0].Action = "Изменённый текст";

        Assert.AreEqual("Проверка", original.Action);
        Assert.AreEqual("Есть", original.Result);
        Assert.AreEqual("Региональный", draft[0].Algorithm);
        Assert.AreEqual("Не проверено", draft[0].Result);
    }

    [TestMethod]
    public void LegacyUnpaidStatistics_FiltersAsCityNotRegional()
    {
        var statistics = new ChecklistCase
        {
            Category = "Статистика не оплативших", Id = "AND-049"
        };
        ChecklistCatalogRules.Normalize(statistics);

        Assert.IsTrue(ChecklistCatalogRules.MatchesFareAlgorithm(
            statistics, ChecklistCatalogRules.CityAlgorithm));
        Assert.IsFalse(ChecklistCatalogRules.MatchesFareAlgorithm(
            statistics, ChecklistCatalogRules.RegionalAlgorithm));
        Assert.IsTrue(ChecklistCatalogRules.MatchesFareAlgorithm(
            statistics, "Оба алгоритма"));
    }

    [TestMethod]
    public void CatalogEditor_CatchesDuplicateIdsCaseInsensitively()
    {
        var first = new ChecklistCase { Id = "FARE-001" };
        var second = new ChecklistCase { Id = "and-002" };
        var cases = new List<ChecklistCase> { first, second };

        Assert.IsTrue(ChecklistCatalogRules.HasDuplicateId(cases, "fare-001", second));
        Assert.IsFalse(ChecklistCatalogRules.HasDuplicateId(cases, "FARE-001", first));
    }
}
