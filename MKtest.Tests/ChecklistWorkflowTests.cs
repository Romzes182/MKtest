using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Checklists;

namespace MKtest.Tests;

[TestClass]
public sealed class ChecklistWorkflowTests
{
    [TestMethod]
    public void BuiltInCatalog_LoadsAll246OriginalChecksWithBlankResults()
    {
        var cases = BuiltInChecklistCatalog.Load();

        Assert.AreEqual(246, cases.Count);
        Assert.IsTrue(cases.Any(x =>
            x.Id.Contains("AND-055") && x.Action.Contains("латиницу")));
        Assert.IsTrue(cases.Any(x =>
            x.Platforms.Contains("Linux") && x.Platforms.Contains("Android")));
        Assert.IsTrue(cases.Any(x => x.Category.Contains("МЧС")));
        Assert.IsTrue(cases.Any(x => x.Category.Contains("Безбилет")));
        Assert.IsTrue(cases.All(x => !string.IsNullOrWhiteSpace(x.Id) &&
                                     !string.IsNullOrWhiteSpace(x.Action) &&
                                     x.Result == "Не проверено"));
    }

    [TestMethod]
    public void NewRun_CopiesChecksButDoesNotCopyPreviousCheckmarks()
    {
        var original = new ChecklistDocument
        {
            Title = "Общий",
            Cases = BuiltInChecklistCatalog.Load()
        };
        original.Cases[0].Result = "Баг";
        original.Cases[0].Bug = "Б-42";

        ChecklistDocument next = original.CreateSubset(original.Cases.Take(2),
            "Повторный проход");

        Assert.AreEqual(2, next.Cases.Count);
        Assert.AreNotEqual(original.RunId, next.RunId);
        Assert.AreEqual("Не проверено", next.Cases[0].Result);
        Assert.AreEqual("", next.Cases[0].Bug);
        Assert.AreEqual(original.Cases[0].Id, next.Cases[0].Id);
        Assert.AreEqual("Баг", original.Cases[0].Result);
    }

    [TestMethod]
    public void RunStore_PreservesPreviousRunsAndResultsSeparately()
    {
        string temp = Path.Combine(Path.GetTempPath(),
            "MKtest-ChecklistTest-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new ChecklistRunStore(temp);
            var first = new ChecklistDocument
            {
                Title = "Проверка Android",
                Cases = new List<ChecklistCase>
                {
                    new() { Id = "AND-001", Action = "Проверить дату", Result = "Баг",
                        Bug = "Б-1", Notes = "Время не обновляется" }
                }
            };
            store.Save(first);
            var second = first.CreateSubset(first.Cases, "Повторный тест");
            second.Cases[0].Result = "Есть";
            store.Save(second);

            var restored = store.Load(first.RunId);
            Assert.AreEqual("Баг", restored.Cases[0].Result);
            Assert.AreEqual("Б-1", restored.Cases[0].Bug);
            Assert.AreEqual("Время не обновляется", restored.Cases[0].Notes);

            Assert.AreEqual("Есть", store.Load(second.RunId).Cases[0].Result);
            Assert.AreEqual(2, store.List().Count);
        }
        finally
        {
            if (Directory.Exists(temp))
                Directory.Delete(temp, recursive: true);
        }
    }
}
