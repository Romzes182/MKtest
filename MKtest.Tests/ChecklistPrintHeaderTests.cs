using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Checklists;

namespace MKtest.Tests;

[TestClass]
public sealed class ChecklistPrintHeaderTests
{
    [TestMethod]
    public void Android7Run_PrintsAndroid7RatherThanCatalogPlatformUnion()
    {
        var run = new ChecklistDocument
        {
            RunPlatform = "Android 7",
            Environment = "Медиапанель №1",
            Cases = new List<ChecklistCase>
            {
                new() { Platforms = "Android 7, Android 11, Linux" },
                new() { Platforms = "Android 11" }
            }
        };
        Assert.AreEqual("Android 7",
            ChecklistPrintLayout.RunOperatingSystem(run, run.Cases));
        Assert.AreEqual("Android 7, встроенный плеер медиапанели, TVS GPS",
            ChecklistPrintLayout.HeaderEnvironment(run, run.Cases));
    }

    [TestMethod]
    public void Android11Run_DoesNotInheritAndroid7FromCases()
    {
        var run = new ChecklistDocument
        {
            RunPlatform = "Android 11",
            Cases = new List<ChecklistCase>
            {
                new() { Platforms = "Linux, Android 7, Android 11" }
            }
        };
        Assert.AreEqual("Android 11",
            ChecklistPrintLayout.RunOperatingSystem(run, run.Cases));
    }

    [TestMethod]
    public void LegacyAndroid7Run_UsesEnvironmentWhenPlatformFieldMissing()
    {
        var run = new ChecklistDocument
        {
            Environment = "Android 7",
            Cases = new List<ChecklistCase>
            {
                new() { Platforms = "Android 7, Android 11, Linux" }
            }
        };
        Assert.AreEqual("Android 7",
            ChecklistPrintLayout.RunOperatingSystem(run, run.Cases));
    }

    [TestMethod]
    public void CustomHeader_TransfersToNewRunWithoutSharingMutableHeader()
    {
        var original = new ChecklistDocument
        {
            RunPlatform = "Android 7",
            PrintHeader = new ChecklistPrintHeader
            {
                Project = "МК Android",
                Purpose = "Регрессионная проверка",
                EnvironmentDetails = "TVS GPS",
                DataValidation = "Контроль байтов"
            }
        };
        ChecklistDocument another = original.CreateSubset(
            new[] { new ChecklistCase { Id = "AND-01" } }, "Повторный обход");
        Assert.AreEqual("Android 7", another.RunPlatform);
        Assert.AreEqual("Регрессионная проверка", another.PrintHeader.Purpose);
        Assert.AreNotSame(original.PrintHeader, another.PrintHeader);
        another.PrintHeader.Purpose = "Новая цель";
        Assert.AreEqual("Регрессионная проверка", original.PrintHeader.Purpose);
    }

    [TestMethod]
    public void Header_CoversAllSectionsFromSuppliedReportExample()
    {
        var header = new ChecklistPrintHeader();
        CollectionAssert.AreEqual(new[]
        {
            "Базовые виджеты", "Автоинформаторы", "Медиа",
            "Служебные данные", "Протоколы оплаты", "Надежность"
        }, header.ScopeRows().Select(x => x.Caption).ToArray());
        CollectionAssert.AreEqual(new[]
        {
            "Настройка тестового окружения",
            "Работа с прикладными протоколами", "Валидация данных"
        }, header.ToolRows().Select(x => x.Caption).ToArray());
    }
}
