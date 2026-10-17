using MKtest.Services.Demoscripts;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKtest.UI
{
    /// <summary>WinForms actions for automatic and reversible manual Demo modes.</summary>
    public sealed class DemoModeController : IDisposable
    {
        private readonly RadioButton _autoMode;
        private readonly RadioButton _manualMode;
        private readonly Button _startButton;
        private readonly Button _nextButton;
        private readonly Button _previousButton;
        private readonly ComboBox _scenarios;
        private readonly IDemoScenarioService _scenarioService;
        private readonly Action<string> _log;

        private DemoNavigationQueue _navigation = new();
        private bool _disposed;

        public DemoModeController(
            RadioButton autoMode,
            RadioButton manualMode,
            Button startButton,
            Button nextButton,
            Button previousButton,
            ComboBox scenarios,
            IDemoScenarioService scenarioService,
            Action<string> log)
        {
            _autoMode = autoMode ?? throw new ArgumentNullException(nameof(autoMode));
            _manualMode = manualMode ?? throw new ArgumentNullException(nameof(manualMode));
            _startButton = startButton ?? throw new ArgumentNullException(nameof(startButton));
            _nextButton = nextButton ?? throw new ArgumentNullException(nameof(nextButton));
            _previousButton = previousButton ?? throw new ArgumentNullException(nameof(previousButton));
            _scenarios = scenarios ?? throw new ArgumentNullException(nameof(scenarios));
            _scenarioService = scenarioService ?? throw new ArgumentNullException(nameof(scenarioService));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _autoMode.CheckedChanged += ModeChanged;
            _manualMode.CheckedChanged += ModeChanged;
            _startButton.Click += StartClicked;
            _nextButton.Click += NextClicked;
            _previousButton.Click += PreviousClicked;

            _autoMode.Checked = true;
            UpdateMode();
        }

        private void ModeChanged(object? sender, EventArgs e) => UpdateMode();

        private void UpdateMode()
        {
            _nextButton.Visible = _manualMode.Checked;
            _previousButton.Visible = _manualMode.Checked;
        }

        private void StartClicked(object? sender, EventArgs e)
        {
            var name = _scenarios.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(name))
            {
                _log("Ошибка: не выбран сценарий");
                return;
            }
            if (_scenarioService.IsDemoRunning)
                return;

            if (_manualMode.Checked)
            {
                _navigation = new DemoNavigationQueue();
                _log($"Запуск сценария: {name} (ручной режим)");
                Func<Task<ManualStepDirection>> navigation = () => _navigation.NextAsync();
                _scenarioService.StartScenarioManual(name, navigation);
            }
            else
            {
                _log($"Запуск сценария: {name}");
                _scenarioService.StartScenario(name);
            }
        }

        private void NextClicked(object? sender, EventArgs e) => Signal(ManualStepDirection.Next);
        private void PreviousClicked(object? sender, EventArgs e) => Signal(ManualStepDirection.Previous);

        private void Signal(ManualStepDirection direction)
        {
            if (_disposed || !_manualMode.Checked || !_scenarioService.IsDemoRunning)
                return;

            _navigation.Enqueue(direction);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _autoMode.CheckedChanged -= ModeChanged;
            _manualMode.CheckedChanged -= ModeChanged;
            _startButton.Click -= StartClicked;
            _nextButton.Click -= NextClicked;
            _previousButton.Click -= PreviousClicked;
        }
    }
}
