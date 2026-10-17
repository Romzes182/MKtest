using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.Demoscripts
{
    /// <summary>
    /// Reversible manual traversal through actual JSON-file steps (delays are skipped).
    /// Back restores the JSON that existed before the current step. Forward replays it.
    /// Retains up to 500 steps of history to bound the memory used by JSON snapshots.
    /// </summary>
    public sealed class ManualDemoScenarioRunner
    {
        private const int HistoryLimit = 500;
        private readonly IDemoStepExecutor _stepExecutor;

        private sealed class Entry
        {
            public ScenarioStep Step { get; }
            public ManualStepCheckpoint Checkpoint { get; set; }

            public Entry(ScenarioStep step, ManualStepCheckpoint checkpoint)
            {
                Step = step;
                Checkpoint = checkpoint;
            }
        }

        public ManualDemoScenarioRunner(IDemoStepExecutor stepExecutor)
        {
            _stepExecutor = stepExecutor ?? throw new ArgumentNullException(nameof(stepExecutor));
        }

        public async Task RunAsync(
            IScenario scenario,
            DemoRunOptions options,
            CancellationToken token)
        {
            var once = scenario.Steps
                .Where(s => s.ExecutionMode == StepExecutionMode.Once && IsFileStep(s))
                .ToArray();
            var loop = scenario.Steps
                .Where(s => s.ExecutionMode == StepExecutionMode.PerLoop && IsFileStep(s))
                .ToArray();

            if (once.Length == 0 && loop.Length == 0)
                return;

            var history = new List<Entry>();
            int current = -1;
            int nextIndex = 0;
            var totalUnique = once.Length + loop.Length;
            var playback = new DemoRunOptions(false, null, options.ReportProgress);

            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (current + 1 < history.Count)
                {
                    ++current;
                    var existing = history[current];
                    existing.Checkpoint = await ManualStepCheckpoint.CaptureAsync(
                        existing.Step.TargetFile, token);
                    await _stepExecutor.ExecuteAsync(existing.Step, scenario.Name, playback, token);
                }
                else
                {
                    if (nextIndex >= totalUnique)
                    {
                        if (loop.Length == 0) return;
                        nextIndex = once.Length;
                    }

                    var step = nextIndex < once.Length
                        ? once[nextIndex]
                        : loop[nextIndex - once.Length];
                    ++nextIndex;

                    var snapshot = await ManualStepCheckpoint.CaptureAsync(step.TargetFile, token);
                    history.Add(new Entry(step, snapshot));
                    ++current;
                    await _stepExecutor.ExecuteAsync(step, scenario.Name, playback, token);

                    if (history.Count > HistoryLimit)
                    {
                        history.RemoveAt(0);
                        --current;
                    }
                }

                options.ReportProgress(
                    $"Ручной шаг: {history[current].Step.Description}");

                while (true)
                {
                    if (options.WaitForDirection == null)
                        return;

                    var direction = await options.WaitForDirection().WaitAsync(token);
                    if (direction == ManualStepDirection.Next)
                        break;

                    if (current == 0)
                    {
                        options.ReportProgress("Это первый доступный шаг истории");
                        continue;
                    }

                    await history[current].Checkpoint.RestoreAsync(token);
                    --current;
                    options.ReportProgress(
                        $"Назад: {history[current].Step.Description}");
                    // Step has already been restored by the saved checkpoint.
                }
            }
        }

        private static bool IsFileStep(ScenarioStep step) =>
            !string.IsNullOrEmpty(step.SourceFile) &&
            !string.IsNullOrEmpty(step.TargetFile);
    }
}
