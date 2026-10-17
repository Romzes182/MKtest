using MKtest.Services.Demoscripts;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKtest.UI
{
    /// <summary>
    /// Единственный обработчик запуска Demo: переключает автоматический и ручной режимы.
    /// Выполнение сценария делегируется интерфейсу IDemoScenarioService.
    /// </summary>
    public sealed class DemoModeController : IDisposable
    {
        private readonly RadioButton _autoMode;
        private readonly RadioButton _manualMode;
        private readonly Button _startButton;
        private readonly Button _nextButton;
        private readonly ComboBox _scenarios;
        private readonly IDemoScenarioService _scenarioService;
        private readonly Action<string> _log;
        private TaskCompletionSource<bool> _nextStepSignal = NewStepSignal();
        private bool _disposed;

        public DemoModeController(
            RadioButton autoMode,
            RadioButton manualMode,
            Button startButton,
            Button nextButton,
            ComboBox scenarios,
            IDemoScenarioService scenarioService,
            Action<string> log)
        {
            _autoMode = autoMode ?? throw new ArgumentNullException(nameof(autoMode));
            _manualMode = manualMode ?? throw new ArgumentNullException(nameof(manualMode));
            _startButton = startButton ?? throw new ArgumentNullException(nameof(startButton));
            _nextButton = nextButton ?? throw new ArgumentNullException(nameof(nextButton));
            _scenarios = scenarios ?? throw new ArgumentNullException(nameof(scenarios));
            _scenarioService = scenarioService ?? throw new ArgumentNullException(nameof(scenarioService));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _autoMode.CheckedChanged += ModeChanged;
            _manualMode.CheckedChanged += ModeChanged;
            _startButton.Click += StartClicked;
            _nextButton.Click += NextClicked;

            _autoMode.Checked = true;
            UpdateMode();
        }

        private void ModeChanged(object? sender, EventArgs e) => UpdateMode();

        private void UpdateMode()
        {
            _nextButton.Visible = _manualMode.Checked;
        }

        private void StartClicked(object? sender, EventArgs e)
        {
            var scenarioName = _scenarios.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(scenarioName))
            {
                _log("Ошибка: не выбран сценарий");
                return;
            }

            if (_scenarioService.IsDemoRunning)
                return;

            if (_manualMode.Checked)
            {
                _nextStepSignal = NewStepSignal();
                _log($"Запуск сценария: {scenarioName} (ручной режим)");
                _scenarioService.StartScenarioManual(scenarioName, () => _nextStepSignal.Task);
            }
            else
            {
                _log($"Запуск сценария: {scenarioName}");
                _scenarioService.StartScenario(scenarioName);
            }
        }

        private void NextClicked(object? sender, EventArgs e)
        {
            if (!_manualMode.Checked || !_scenarioService.IsDemoRunning)
                return;

            var signal = _nextStepSignal;
            _nextStepSignal = NewStepSignal();
            signal.TrySetResult(true);
        }

        private static TaskCompletionSource<bool> NewStepSignal() =>
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Dispose()
        {
            if (_disposed) return;
            _autoMode.CheckedChanged -= ModeChanged;
            _manualMode.CheckedChanged -= ModeChanged;
            _startButton.Click -= StartClicked;
            _nextButton.Click -= NextClicked;
            _disposed = true;
        }
    }
}
