using MKtest.Services.Demoscripts;
using System;
using System.Windows.Forms;

namespace MKtest.Managers
{
    public class DemoScenarioManager
    {
        private readonly ComboBox _comboBox;
        private readonly Button _startButton;
        private readonly Button _stopButton;
        private readonly Label _statusLabel;
        private readonly LogManager _logManager;
        private readonly IDemoScenarioService _demoService;

        public DemoScenarioManager(
            ComboBox comboBox,
            Button startButton,
            Button stopButton,
            Label statusLabel,
            LogManager logManager,
            IDemoScenarioService demoService)
        {
            _comboBox = comboBox ?? throw new ArgumentNullException(nameof(comboBox));
            _startButton = startButton ?? throw new ArgumentNullException(nameof(startButton));
            _stopButton = stopButton ?? throw new ArgumentNullException(nameof(stopButton));
            _statusLabel = statusLabel ?? throw new ArgumentNullException(nameof(statusLabel));
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
            _demoService = demoService ?? throw new ArgumentNullException(nameof(demoService));

            Initialize();
        }

        private void Initialize()
        {
            LoadScenarios();
            SetupEventHandlers();
            UpdateUI(false);
        }

        private void LoadScenarios()
        {
            _comboBox.Items.Clear();
            foreach (var scenario in _demoService.AvailableScenarios)
            {
                _comboBox.Items.Add(scenario);
            }

            if (_comboBox.Items.Count > 0)
                _comboBox.SelectedIndex = 0;
        }

        private void SetupEventHandlers()
        {
            _stopButton.Click += StopButton_Click;
            _comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            _demoService.DemoStatusChanged += DemoService_DemoStatusChanged;
            _demoService.ScenarioProgress += DemoService_ScenarioProgress;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            AppendLog("Остановка демо-сценария");
            _demoService.StopScenario();
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedScenario = _comboBox.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedScenario))
            {
                AppendLog($"Выбран сценарий: {selectedScenario}");
            }
        }

        private void DemoService_DemoStatusChanged(object? sender, bool isRunning)
        {
            UpdateUI(isRunning);
            AppendLog($"Статус демо изменен: {(isRunning ? "запущено" : "остановлено")}");
        }

        private void DemoService_ScenarioProgress(object? sender, string message)
        {
            AppendLog($"Прогресс: {message}");
        }

        private void UpdateUI(bool isRunning)
        {
            _statusLabel.Invoke((MethodInvoker)(() =>
            {
                _statusLabel.Text = isRunning ? "Демо запущено" : "Демо не запущено";
                _statusLabel.ForeColor = isRunning ? System.Drawing.Color.Green : System.Drawing.Color.Red;
                _startButton.Enabled = !isRunning;
                _stopButton.Enabled = isRunning;
            }));
        }

        private void AppendLog(string message) => _logManager.AppendLog(message);

        public void Cleanup()
        {
            _demoService.StopScenario();
            _stopButton.Click -= StopButton_Click;
            _comboBox.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
            _demoService.DemoStatusChanged -= DemoService_DemoStatusChanged;
            _demoService.ScenarioProgress -= DemoService_ScenarioProgress;
        }
    }
}