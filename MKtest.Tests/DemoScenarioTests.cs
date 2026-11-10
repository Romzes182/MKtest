using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Configs;
using MKtest.Services.Demoscripts;
using System.Text.Json;

namespace MKtest.Tests;

[TestClass]
[DoNotParallelize]
public sealed class DemoScenarioTests
{
    private string _root = null!;
    private string _originalDirectory = null!;

    [TestInitialize]
    public void Initialize()
    {
        _originalDirectory = Environment.CurrentDirectory;
        _root = Path.Combine(Path.GetTempPath(), "MKtest-Demo-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        Environment.CurrentDirectory = _root;
    }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.CurrentDirectory = _originalDirectory;
        Directory.Delete(_root, recursive: true);
    }

    [TestMethod]
    public void DefaultScenarioPath_DoesNotDependOnWorkingDirectory()
    {
        var files = new DemoFileService(new DemoConfig());

        Assert.AreEqual(Path.Combine(AppContext.BaseDirectory, "DemoScenarios", "Demo1"),
            files.GetScenarioPath("demo-1"));
        Assert.AreEqual(Path.Combine(AppContext.BaseDirectory, "JsonTemplates", "route.json"),
            files.GetTemplatePath("route.json"));
        Assert.IsFalse(Directory.Exists(Path.Combine(_root, "DemoScenarios")));
        Assert.IsFalse(Directory.Exists(Path.Combine(_root, "JsonTemplates")));
    }

    [TestMethod]
    public void RelativeConfiguredPaths_AreResolvedFromApplicationDirectory()
    {
        var config = CreateConfig();
        config.ScenariosPath = RelativeToApplication(config.ScenariosPath);
        config.TemplatesPath = RelativeToApplication(config.TemplatesPath);
        var files = new DemoFileService(config);

        Assert.AreEqual(Path.Combine(_root, "scenarios", "Demo1"), files.GetScenarioPath("demo-1"));
        Assert.AreEqual(Path.Combine(_root, "templates", "route.json"), files.GetTemplatePath("route.json"));
    }

    [TestMethod]
    public async Task CopyStep_UsesConfiguredTemplateDirectory()
    {
        var config = CreateConfig();
        var files = new DemoFileService(config);
        Directory.CreateDirectory(files.GetScenarioPath("demo-1"));
        const string json = "{\"number\":\"1\"}";
        File.WriteAllText(Path.Combine(files.GetScenarioPath("demo-1"), "source.json"), json);
        var errors = new List<string>();
        var executor = new DemoStepExecutor(files, errors.Add);

        await executor.ExecuteAsync(new ScenarioStep
        {
            SourceFile = "source.json",
            TargetFile = "route.json"
        }, "demo-1", new DemoRunOptions(false, null, _ => { }), CancellationToken.None);

        var target = Path.Combine(config.TemplatesPath, "route.json");
        Assert.IsTrue(File.Exists(target), string.Join(Environment.NewLine, errors));
        Assert.AreEqual(json, File.ReadAllText(target));
    }

    [TestMethod]
    [DataRow("demo-1")]
    [DataRow("demo-2")]
    [DataRow("demo-3")]
    [DataRow("demo-4")]
    [DataRow("demo-5")]
    [DataRow("demo-45")]
    public void BuiltInScenario_AllReferencedFilesExistAndContainJson(string scenarioName)
    {
        var catalog = new DemoScenarioCatalog();
        Assert.IsTrue(catalog.TryGet(scenarioName, out var scenario));
        var files = new DemoFileService(new DemoConfig
        {
            ScenariosPath = Path.Combine(AppContext.BaseDirectory, "DemoScenarios"),
            TemplatesPath = Path.Combine(_root, "templates")
        });

        foreach (var step in scenario.Steps.Where(s => !string.IsNullOrEmpty(s.SourceFile)))
        {
            var path = Path.Combine(files.GetScenarioPath(scenarioName), step.SourceFile);
            Assert.IsTrue(File.Exists(path), $"{scenarioName}: отсутствует {step.SourceFile}");
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            Assert.AreEqual(JsonValueKind.Object, document.RootElement.ValueKind);
        }
    }

    [TestMethod]
    [DataRow("demo-1", 11, 20000)]
    [DataRow("demo-4", 10, 10000)]
    public void MissingCoordinates_DoNotRemoveInformatorStepsOrDelays(
        string scenarioName, int informatorSteps, int delayMs)
    {
        var catalog = new DemoScenarioCatalog();
        Assert.IsTrue(catalog.TryGet(scenarioName, out var scenario));
        Assert.AreEqual(1, scenario.Steps.Count(s => s.ExecutionMode == StepExecutionMode.Once));
        var loop = scenario.Steps.Where(s => s.ExecutionMode == StepExecutionMode.PerLoop).ToArray();
        int index = 0;
        var number = scenarioName.Substring("demo-".Length);
        for (int i = 1; i <= informatorSteps; i++)
        {
            Assert.AreEqual($"{number}i{i}.json", loop[index].SourceFile);
            Assert.AreEqual("informator.json", loop[index++].TargetFile);
            if (i <= 6)
            {
                Assert.AreEqual($"coord{number}{i}.json", loop[index].SourceFile);
                Assert.AreEqual("coord.json", loop[index++].TargetFile);
            }
            Assert.AreEqual(string.Empty, loop[index].SourceFile);
            Assert.AreEqual(delayMs, loop[index++].DelayMs);
        }
        Assert.AreEqual(loop.Length, index);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task ManualNavigation_RestoresAndReplaysFilesInConfiguredDirectory(bool targetExisted)
    {
        var files = new DemoFileService(CreateConfig());
        var source = files.GetScenarioPath("demo-1");
        Directory.CreateDirectory(source);
        File.WriteAllText(Path.Combine(source, "route.json"), "{\"route\":1}");
        File.WriteAllText(Path.Combine(source, "first.json"), "{\"stop\":1}");
        File.WriteAllText(Path.Combine(source, "second.json"), "{\"stop\":2}");
        var target = files.GetTemplatePath("informator.json");
        if (targetExisted)
            File.WriteAllText(target, "{\"stop\":0}");

        var scenario = new TestScenario(new[]
        {
            FileStep("route.json", "route.json"),
            FileStep("first.json", "informator.json"),
            FileStep("second.json", "informator.json")
        });
        var log = new List<string>();
        var runner = new DemoScenarioRunner(new DemoStepExecutor(files, log.Add), files.GetTemplatePath);
        int navigationCalls = 0;
        var options = new DemoRunOptions(true, null, _ => { })
        {
            WaitForDirection = () =>
            {
                // Next, Back, replay, Next, Back, replay, finish.
                int call = navigationCalls++;
                if (call is 0 or 2)
                {
                    Assert.AreEqual(targetExisted, File.Exists(target));
                    if (targetExisted)
                        Assert.AreEqual("{\"stop\":0}", File.ReadAllText(target));
                }
                else
                {
                    Assert.AreEqual(call is 4 or 6 ? "{\"stop\":2}" : "{\"stop\":1}",
                        File.ReadAllText(target));
                }
                Assert.AreEqual("{\"route\":1}", File.ReadAllText(files.GetTemplatePath("route.json")));
                return Task.FromResult(call is 1 or 4 ? ManualStepDirection.Previous : ManualStepDirection.Next);
            }
        };

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await runner.RunAsync(scenario, options, timeout.Token);

        Assert.AreEqual(7, navigationCalls);
        Assert.AreEqual("{\"stop\":2}", File.ReadAllText(target));
        Assert.IsFalse(log.Any(line => line.IndexOf("Ошибка", StringComparison.Ordinal) >= 0));
        Assert.IsFalse(Directory.Exists(Path.Combine(_root, "JsonTemplates")));
    }

    [TestMethod]
    public async Task MissingSource_DoesNotChangeTargetOrWaitForNext()
    {
        var files = new DemoFileService(CreateConfig());
        var target = files.GetTemplatePath("coord.json");
        const string previous = "{\"latitude\":59.8}";
        File.WriteAllText(target, previous);
        var log = new List<string>();
        int waits = 0;
        var options = new DemoRunOptions(true, () =>
        {
            waits++;
            return Task.CompletedTask;
        }, _ => { });

        await new DemoStepExecutor(files, log.Add).ExecuteAsync(
            FileStep("missing.json", "coord.json"), "demo-1", options, CancellationToken.None);

        Assert.AreEqual(previous, File.ReadAllText(target));
        Assert.AreEqual(0, waits);
        StringAssert.Contains(string.Join(Environment.NewLine, log),
            Path.Combine(files.GetScenarioPath("demo-1"), "missing.json"));
    }

    [TestMethod]
    public async Task ManualWait_CanBeCancelledWithoutAnotherClick()
    {
        var files = new DemoFileService(CreateConfig());
        Directory.CreateDirectory(files.GetScenarioPath("demo-1"));
        File.WriteAllText(Path.Combine(files.GetScenarioPath("demo-1"), "source.json"), "{}");
        var waiting = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var neverClicked = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var options = new DemoRunOptions(true, () =>
        {
            waiting.SetResult(true);
            return neverClicked.Task;
        }, _ => { });
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var execution = new DemoStepExecutor(files, _ => { }).ExecuteAsync(
            FileStep("source.json", "route.json"), "demo-1", options, cancellation.Token);
        try
        {
            await WaitForCompletionAsync(waiting.Task);
            cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => WaitForCompletionAsync(execution));
        }
        finally
        {
            cancellation.Cancel();
            try { await execution; }
            catch (OperationCanceledException) { }
        }
    }

    private static string RelativeToApplication(string path) =>
        Uri.UnescapeDataString(new Uri(AppContext.BaseDirectory).MakeRelativeUri(new Uri(path)).ToString())
            .Replace('/', Path.DirectorySeparatorChar);

    private static async Task WaitForCompletionAsync(Task task)
    {
        if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5))) != task)
            throw new TimeoutException("The operation did not finish in time.");
        await task;
    }

    private static ScenarioStep FileStep(string source, string target) => new()
    {
        SourceFile = source,
        TargetFile = target,
        ExecutionMode = StepExecutionMode.Once
    };

    private sealed class TestScenario(IReadOnlyList<ScenarioStep> steps) : IScenario
    {
        public string Name => "demo-1";
        public string Description => "Test scenario";
        public IReadOnlyList<ScenarioStep> Steps { get; } = steps;
    }

    private DemoConfig CreateConfig() => new()
    {
        ScenariosPath = Path.Combine(_root, "scenarios"),
        TemplatesPath = Path.Combine(_root, "templates")
    };
}
