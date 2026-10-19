using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Checklists;

namespace MKtest.Tests;

[TestClass]
public sealed class ChecklistCatalogPlacementTests
{
    private static List<ChecklistCase> Example() => new()
    {
        new() { Id = "FARE-001", Category = "Безбилетники",
            Algorithm = "СПб ГКУ" },
        new() { Id = "FARE-002", Category = "Безбилетники",
            Algorithm = "СПб ГКУ" },
        new() { Id = "FARE-003", Category = "Безбилетники",
            Algorithm = "Региональный" },
        new() { Id = "FARE-004", Category = "Безбилетники",
            Algorithm = "Региональный" },
        new() { Id = "MCH-001", Category = "МЧС" }
    };

    [TestMethod]
    public void AddToCityAlgorithm_InsertsAfterChosenCityCheck()
    {
        var list = Example();
        var test = new ChecklistCase { Id = "USER-001", Category = "Безбилетники",
            Algorithm = "СПб ГКУ" };

        int position = ChecklistCatalogPlacement.InsertIndex(list, test, "FARE-001");
        list.Insert(position, test);

        CollectionAssert.AreEqual(
            new[] { "FARE-001", "USER-001", "FARE-002", "FARE-003", "FARE-004", "MCH-001" },
            list.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void AddToRegionalAlgorithm_ByDefaultGoesAfterLastRegionalCheck()
    {
        var list = Example();
        var test = new ChecklistCase { Id = "USER-002", Category = "Безбилетники",
            Algorithm = "Региональный" };
        int position = ChecklistCatalogPlacement.InsertIndex(list, test);
        list.Insert(position, test);

        CollectionAssert.AreEqual(
            new[] { "FARE-001", "FARE-002", "FARE-003", "FARE-004", "USER-002", "MCH-001" },
            list.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void NewCategory_UsesEndOfCatalogIfGroupDoesNotExist()
    {
        var list = Example();
        int position = ChecklistCatalogPlacement.InsertIndex(list,
            new ChecklistCase { Category = "Новая группа" });
        Assert.AreEqual(list.Count, position);
    }

    [TestMethod]
    public void CrossAlgorithmPosition_IsRejectedToPreventWrongPlacement()
    {
        var list = Example();
        var city = new ChecklistCase { Category = "Безбилетники", Algorithm = "СПб ГКУ" };

        Assert.ThrowsExactly<ArgumentException>(() =>
            ChecklistCatalogPlacement.InsertIndex(list, city, "FARE-003"));
    }

    [TestMethod]
    public void CategoryOrAlgorithmChange_MovesEditedCheckToItsNewGroup()
    {
        var list = Example();
        var existing = list[0];
        var edited = existing.NewBlankResult();
        edited.Algorithm = "Региональный";

        Assert.IsFalse(ChecklistCatalogPlacement.SameGroup(existing, edited));
        list.Remove(existing);
        list.Insert(ChecklistCatalogPlacement.InsertIndex(list, edited), edited);
        CollectionAssert.AreEqual(
            new[] { "FARE-002", "FARE-003", "FARE-004", "FARE-001", "MCH-001" },
            list.Select(x => x.Id).ToArray());
    }
}
