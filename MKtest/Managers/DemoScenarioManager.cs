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
        private readonly TextBox _logTextBox;
        private readonly IDemoScenarioService _demoService;

        public DemoScenarioManager(
            ComboBox comboBox,
            Button startButton,
            Button stopButton,
            Label statusLabel,
            TextBox logTextBox,
            IDemoScenarioService demoService)
        {
            _comboBox = comboBox ?? throw new ArgumentNullException(nameof(comboBox));
            _startButton = startButton ?? throw new ArgumentNullException(nameof(startButton));
            _stopButton = stopButton ?? throw new ArgumentNullException(nameof(stopButton));
            _statusLabel = statusLabel ?? throw new ArgumentNullException(nameof(statusLabel));
            _logTextBox = logTextBox ?? throw new ArgumentNullException(nameof(logTextBox));
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
            _startButton.Click += StartButton_Click;
            _stopButton.Click += StopButton_Click;
            _comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            _demoService.DemoStatusChanged += DemoService_DemoStatusChanged;
            _demoService.ScenarioProgress += DemoService_ScenarioProgress;
        }

        public void StartButton_Click(object sender, EventArgs e)
        {
            var selectedScenario = _comboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedScenario))
            {
                AppendLog("Ошибка: не выбран сценарий");
                return;
            }

            AppendLog($"Запуск сценария: {selectedScenario}");
            _demoService.StartScenario(selectedScenario);

        }

        public EventHandler StartButtonHandler => StartButton_Click;

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

        private void AppendLog(string message)
        {
            _logTextBox.Invoke((MethodInvoker)(() =>
            {
                _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
                _logTextBox.ScrollToCaret();
            }));
        }

        public void Cleanup()
        {
            _demoService.StopScenario();
            _demoService.DemoStatusChanged -= DemoService_DemoStatusChanged;
            _demoService.ScenarioProgress -= DemoService_ScenarioProgress;
        }
    }
}