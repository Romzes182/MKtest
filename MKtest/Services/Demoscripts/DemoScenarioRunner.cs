using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.Demoscripts
{
    /// <summary>
    /// Выполняет шаги Once один раз, затем повторяет PerLoop до отмены.
    /// Не знает ни о WinForms, ни о JSON, ни о способе выполнения отдельного шага.
    /// </summary>
    public sealed class DemoScenarioRunner : IDemoScenarioRunner
    {
        private readonly IDemoStepExecutor _steps;

        public DemoScenarioRunner(IDemoStepExecutor steps)
        {
            _steps = steps ?? throw new ArgumentNullException(nameof(steps));
        }

        public async Task RunAsync(
            IScenario scenario,
            DemoRunOptions options,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(scenario);
            ArgumentNullException.ThrowIfNull(options);

            foreach (var step in scenario.Steps.Where(s => s.ExecutionMode == StepExecutionMode.Once))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _steps.ExecuteAsync(step, scenario.Name, options, cancellationToken);
            }

            var loopSteps = scenario.Steps
                .Where(s => s.ExecutionMode == StepExecutionMode.PerLoop)
                .ToArray();

            // A scenario with no repeating steps should finish rather than busy-spin.
            if (loopSteps.Length == 0)
                return;

            while (true)
            {
                foreach (var step in loopSteps)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await _steps.ExecuteAsync(step, scenario.Name, options, cancellationToken);
                }
            }
        }
    }
}
