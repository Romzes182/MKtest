using MKtest.Managers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.Demoscripts
{
    /// <summary>
    /// Координатор жизненного цикла Demo-сценария.
    /// Каталог, исполнение шагов и правила повторения вынесены в отдельные классы.
    /// </summary>
    public sealed class DemoScenarioService : IDemoScenarioService
    {
        private readonly IDemoScenarioCatalog _catalog;
        private readonly IDemoScenarioRunner _runner;
        private readonly LogManager _logManager;
        private readonly object _sync = new();
        private CancellationTokenSource? _activeRun;
        private Task? _activeTask;

        public bool IsDemoRunning
        {
            get
            {
                lock (_sync)
                    return _activeRun != null;
            }
        }

        public IReadOnlyList<string> AvailableScenarios => _catalog.Names;

        public event EventHandler<string>? ScenarioProgress;
        public event EventHandler<bool>? DemoStatusChanged;

        // Existing MainForm constructor is preserved.
        public DemoScenarioService(IDemoFileService fileService, LogManager logManager)
            : this(
                new DemoScenarioCatalog(),
                CreateDefaultRunner(fileService, logManager),
                logManager)
        {
        }

        public DemoScenarioService(
            IDemoScenarioCatalog catalog,
            IDemoScenarioRunner runner,
            LogManager logManager)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _runner = runner ?? throw new ArgumentNullException(nameof(runner));
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
            _logManager.AppendLog("Демо-сервис инициализирован");
        }

        private static IDemoScenarioRunner CreateDefaultRunner(
            IDemoFileService fileService, LogManager logManager) =>
            new DemoScenarioRunner(new DemoStepExecutor(
                fileService ?? throw new ArgumentNullException(nameof(fileService)),
                (logManager ?? throw new ArgumentNullException(nameof(logManager))).AppendLog));

        public void StartScenario(string scenarioName) =>
            StartCore(scenarioName, manualMode: false, waitForNext: null);

        public void StartScenarioManual(string scenarioName, Func<Task> waitTaskProvider) =>
            StartCore(scenarioName, manualMode: true, waitForNext: waitTaskProvider);

        /// <summary>Reversible manual navigation; existing overload remains supported.</summary>
        public void StartScenarioManual(
            string scenarioName,
            Func<Task<ManualStepDirection>> navigationProvider) =>
            StartCore(scenarioName, manualMode: true, waitForNext: null,
                navigationProvider: navigationProvider);

        private void StartCore(string scenarioName, bool manualMode, Func<Task>? waitForNext,
            Func<Task<ManualStepDirection>>? navigationProvider = null)
        {
            CancellationTokenSource source;
            IScenario scenario;

            lock (_sync)
            {
                if (_activeRun != null)
                {
                    _logManager.AppendLog("Демо уже запущено. Сначала остановите текущий сценарий.");
                    return;
                }

                if (!_catalog.TryGet(scenarioName, out scenario!))
                {
                    _logManager.AppendLog($"Сценарий '{scenarioName}' не найден.");
                    return;
                }

                source = new CancellationTokenSource();
                _activeRun = source;
            }

            var options = new DemoRunOptions(manualMode, waitForNext,
                message => ScenarioProgress?.Invoke(this, message))
            {
                WaitForDirection = navigationProvider
            };

            DemoStatusChanged?.Invoke(this, true);
            _activeTask = Task.Run(() => RunScenarioAsync(scenario, options, source));
            _logManager.AppendLog($"Сценарий '{scenarioName}' запущен.");
        }

        public void StopScenario()
        {
            lock (_sync)
            {
                if (_activeRun == null || _activeRun.IsCancellationRequested)
                    return;

                _activeRun.Cancel();
            }

            _logManager.AppendLog("Демо сценарий остановлен.");
        }

        /// <summary>Cancel and await the running demo before the UI logs are disposed.</summary>
        public async Task StopScenarioAsync()
        {
            StopScenario();
            Task? pending;
            lock (_sync)
                pending = _activeTask;

            if (pending != null)
                await pending.ConfigureAwait(false);
        }

        private async Task RunScenarioAsync(
            IScenario scenario, DemoRunOptions options, CancellationTokenSource source)
        {
            try
            {
                await _runner.RunAsync(scenario, options, source.Token);
            }
            catch (OperationCanceledException) when (source.IsCancellationRequested)
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
                lock (_sync)
                {
                    if (ReferenceEquals(_activeRun, source))
                        _activeRun = null;
                }

                source.Dispose();
                DemoStatusChanged?.Invoke(this, false);
            }
        }
    }
}
