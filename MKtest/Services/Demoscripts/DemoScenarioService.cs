using MKtest.Managers;
using MKtest.Services.DemoScenarios;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.Demoscripts
{
    public class DemoScenarioService : IDemoScenarioService
    {
        private readonly IDemoFileService _fileService;
        private readonly LogManager _logManager;
        private readonly Dictionary<string, IScenario> _scenarios;
        private CancellationTokenSource? _cts;
        private bool _isRunning;

        public bool IsDemoRunning => _isRunning;
        public IReadOnlyList<string> AvailableScenarios => _scenarios.Keys.ToList();

        public event EventHandler<string>? ScenarioProgress;
        public event EventHandler<bool>? DemoStatusChanged;

        public DemoScenarioService(IDemoFileService fileService, LogManager logManager)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));

            _scenarios = new Dictionary<string, IScenario>
            {
                ["demo-1"] = new Demo1Scenario(),
                ["demo-2"] = new Demo2Scenario(),
                ["demo-3"] = new Demo3Scenario(),
                ["demo-4"] = new Demo4Scenario(),
                ["demo-5"] = new Demo5Scenario(),
                ["demo-45"] = new Demo45Scenario()
            };

            _logManager.AppendLog("Демо-сервис инициализирован");
        }

        public void StartScenario(string scenarioName)
        {
            if (_isRunning)
            {
                _logManager.AppendLog("Демо уже запущено. Сначала остановите текущий сценарий.");
                return;
            }

            if (!_scenarios.TryGetValue(scenarioName, out var scenario))
            {
                _logManager.AppendLog($"Сценарий '{scenarioName}' не найден.");
                return;
            }

            _isRunning = true;
            DemoStatusChanged?.Invoke(this, true);
            _cts = new CancellationTokenSource();

            Task.Run(() => RunScenarioAsync(scenario, _cts.Token));
            _logManager.AppendLog($"Сценарий '{scenarioName}' запущен.");
        }

        public void StopScenario()
        {
            if (!_isRunning) return;

            _cts?.Cancel();
            _isRunning = false;
            DemoStatusChanged?.Invoke(this, false);
            _logManager.AppendLog("Демо сценарий остановлен.");
        }

        private async Task RunScenarioAsync(IScenario scenario, CancellationToken token)
        {
            try
            {
                await ExecuteOnceStepsAsync(scenario, token);
                await ExecuteLoopStepsAsync(scenario, token);
            }
            catch (OperationCanceledException)
            {
                ScenarioProgress?.Invoke(this, "Сценарий прерван пользователем");
            }
            catch (Exception ex)
            {
                _logManager.AppendLog($"Ошибка выполнения сценария: {ex.Message}");
                ScenarioProgress?.Invoke(this, $"Ошибка: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
                DemoStatusChanged?.Invoke(this, false);
            }
        }

        private async Task ExecuteOnceStepsAsync(IScenario scenario, CancellationToken token)
        {
            foreach (var step in scenario.Steps.Where(s => s.ExecutionMode == StepExecutionMode.Once))
            {
                if (token.IsCancellationRequested) return;
                await ExecuteStepAsync(step, scenario.Name, token);
            }
        }

        private async Task ExecuteLoopStepsAsync(IScenario scenario, CancellationToken token)
        {
            var loopSteps = scenario.Steps.Where(s => s.ExecutionMode == StepExecutionMode.PerLoop).ToList();

            while (!token.IsCancellationRequested)
            {
                foreach (var step in loopSteps)
                {
                    if (token.IsCancellationRequested) break;
                    await ExecuteStepAsync(step, scenario.Name, token);
                }
            }
        }

        private async Task ExecuteStepAsync(ScenarioStep step, string scenarioName, CancellationToken token)
        {
            try
            {
                if (!string.IsNullOrEmpty(step.SourceFile) && !string.IsNullOrEmpty(step.TargetFile))
                {
                    await CopyScenarioFileAsync(step, scenarioName);
                }

                if (step.DelayMs > 0)
                {
                    ScenarioProgress?.Invoke(this, $"Задержка {step.DelayMs}мс: {step.Description}");
                    await Task.Delay(step.DelayMs, token);
                }
            }
            catch (Exception ex)
            {
                _logManager.AppendLog($"Ошибка выполнения шага '{step.Description}': {ex.Message}");
                ScenarioProgress?.Invoke(this, $"Ошибка: {step.Description}");
            }
        }

        private async Task CopyScenarioFileAsync(ScenarioStep step, string scenarioName)
        {
            try
            {
                var scenarioPath = _fileService.GetScenarioPath(scenarioName);
                var sourcePath = Path.Combine(scenarioPath, step.SourceFile);
                var destPath = Path.Combine("JsonTemplates", step.TargetFile);

                await _fileService.CopyFileAsync(sourcePath, destPath);
                ScenarioProgress?.Invoke(this, step.Description);
                _logManager.AppendLog($"[Демо] {step.Description}");
            }
            catch (FileNotFoundException ex)
            {
                _logManager.AppendLog($"Файл не найден: {ex.FileName}");
                throw;
            }
            catch (Exception ex)
            {
                _logManager.AppendLog($"Ошибка копирования файла: {ex.Message}");
                throw;
            }
        }
    }
}