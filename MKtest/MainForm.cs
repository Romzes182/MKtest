using MKtest.Configs;
using MKtest.Managers;
using MKtest.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace MKtest
{
    public partial class MainForm : Form
    {
        #region Менеджеры (объявляем как nullable)
        private LogManager? _logManager;
        private SSHConnectionManager? _sshManager;
        private ConnectionStateManager? _stateManager;
        private TimeCommandsManager? _timeManager;
        private SettingsManager? _settingsManager;
        private DemoScenarioManager? _demoManager;
        #endregion

        #region Сервисы (объявляем как nullable)
        private SSHService? _sshService;
        private TimeCommandsService? _timeService;
        private WebServerService? _webServerService;
        private DemoFileService? _demoFileService;
        private DemoScenarioService? _demoScenarioService;
        #endregion

        #region Веб-сервер менеджеры
        private WebServerManager? _webServerManager;
        private WebServerStateManager? _webServerStateManager;
        #endregion

        #region Поля для управления сворачиванием
        private Dictionary<Panel, int> _originalHeights = new Dictionary<Panel, int>();
        #endregion

        #region Конструктор
        public MainForm()
        {
            InitializeComponent();
            InitializeAll();
        }
        #endregion

        #region Инициализация всего
        private void InitializeAll()
        {
            try
            {
                // Перемещаем logPanel в mainTabPage (нижняя часть)
                RepositionLogPanel();

                // Инициализируем всё в правильном порядке
                InitializeServices();
                InitializeManagers();
                SetupUI();

                _logManager?.AppendLog("Приложение инициализировано");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RepositionLogPanel()
        {
            // Убедимся, что logPanel находится в правильном месте
            if (logPanel.Parent != mainTabPage)
            {
                // Удаляем logPanel из текущего родителя
                if (logPanel.Parent != null)
                {
                    logPanel.Parent.Controls.Remove(logPanel);
                }

                // Добавляем logPanel в mainTabPage внизу
                mainTabPage.Controls.Add(logPanel);
                logPanel.Dock = DockStyle.Bottom;
                logPanel.Height = 200;
                logPanel.BringToFront();

                // Устанавливаем splitContainerMain для заполнения оставшегося пространства
                splitContainerMain.Dock = DockStyle.Fill;
                splitContainerMain.BringToFront();
            }
        }
        #endregion

        #region Инициализация сервисов
        private void InitializeServices()
        {
            // Лог менеджер должен быть первым
            _logManager = new LogManager(beelinkLogTextBox);

            _sshService = new SSHService();
            _timeService = new TimeCommandsService(_sshService);
            _webServerService = new WebServerService(_logManager);
        }
        #endregion

        #region Инициализация менеджеров
        private void InitializeManagers()
        {
            // 1. Менеджер состояния SSH UI
            _stateManager = new ConnectionStateManager(
                beelinkConnectButton,
                beelinkDisconnectButton,
                beelinkTestButton,
                beelinkStatusLabel,
                timeCheckButton,
                timeEnableNTPButton,
                timeDisableNTPButton,
                timeSetButton,
                manualDatePicker,
                manualTimePicker,
                timeGroupBox
            );

            // 2. Менеджер состояния веб-сервера
            _webServerStateManager = new WebServerStateManager(
                webServerStartButton,
                webServerStopButton,
                webServerStatusLabel
            );

            // 3. Менеджер настроек
            _settingsManager = new SettingsManager(
                ipTextBox,
                portNumeric,
                userTextBox,
                passwordUserTextBox,
                passwordRootTextBox,
                webServerIpTextBox,
                webServerPortNumeric
            );

            // 4. Менеджер SSH подключения
            if (_sshService == null || _logManager == null || _stateManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");

            _sshManager = new SSHConnectionManager(_sshService, _logManager, _stateManager);

            // 5. Менеджер веб-сервера
            if (_webServerService == null || _logManager == null || _webServerStateManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");

            _webServerManager = new WebServerManager(_webServerService, _logManager, _webServerStateManager);

            // 6. Менеджер команд времени
            if (_timeService == null || _logManager == null || _sshManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");

            _timeManager = new TimeCommandsManager(_timeService, _logManager, () =>
                _sshManager.IsConnected()
            );

            // 7. Инициализация демо-сервисов
            InitializeDemoServices();
        }

        private void InitializeDemoServices()
        {
            try
            {
                // Создаем демо-конфигурацию
                var demoConfig = new DemoConfig();

                // Создаем файловый сервис
                _demoFileService = new DemoFileService(demoConfig);

                // Создаем сервис сценариев
                _demoScenarioService = new DemoScenarioService(_demoFileService, _logManager!);

                // Создаем менеджер UI для демо-сценариев
                _demoManager = new DemoScenarioManager(
                    cmbDemoScenarios,
                    btnStartDemo,
                    btnStopDemo,
                    lblDemoStatus,
                    beelinkLogTextBox,
                    _demoScenarioService
                );

                _logManager?.AppendLog("Демо-сервисы инициализированы");
            }
            catch (Exception ex)
            {
                _logManager?.AppendLog($"Ошибка инициализации демо-сервисов: {ex.Message}");
            }
        }
        #endregion

        #region Настройка UI
        private void SetupUI()
        {
            // Настройка контролов времени
            manualDatePicker.Format = DateTimePickerFormat.Short;
            manualTimePicker.Format = DateTimePickerFormat.Time;
            manualTimePicker.ShowUpDown = true;

            // Сохраняем исходные высоты сворачиваемых панелей
            SaveOriginalHeights();

            // Настройка сворачиваемой панели SSH Beelink
            beelinkHeaderPanel.Click += CollapsiblePanelHeader_Click;
            beelinkHeaderLabel.Click += CollapsiblePanelHeader_Click;
            beelinkCollapsiblePanel.Tag = "beelink";

            // Настройка сворачиваемой панели веб-сервера
            webServerHeaderPanel.Click += CollapsiblePanelHeader_Click;
            webServerHeaderLabel.Click += CollapsiblePanelHeader_Click;
            webServerCollapsiblePanel.Tag = "webserver";

            // Настройка сворачиваемой панели демо-сценариев
            demoHeaderPanel.Click += CollapsiblePanelHeader_Click;
            demoHeaderLabel.Click += CollapsiblePanelHeader_Click;
            demoCollapsiblePanel.Tag = "demo";

            // Обработчики кнопок веб-сервера
            webServerStartButton.Click += WebServerStartButton_Click;
            webServerStopButton.Click += WebServerStopButton_Click;

            // Загрузка настроек
            _settingsManager?.LoadSettings();

            // Добавляем обработчик для обновления расположения при изменении размера формы
            this.Resize += MainForm_Resize;

            // Сворачиваем все панели при запуске
            CollapseAllPanels();
        }

        private void SaveOriginalHeights()
        {
            _originalHeights[beelinkCollapsiblePanel] = beelinkCollapsiblePanel.Height;
            _originalHeights[webServerCollapsiblePanel] = webServerCollapsiblePanel.Height;
            _originalHeights[demoCollapsiblePanel] = demoCollapsiblePanel.Height;
        }

        private void CollapseAllPanels()
        {
            // Сворачиваем SSH Beelink
            beelinkContentPanel.Visible = false;
            beelinkCollapsiblePanel.Height = beelinkHeaderPanel.Height;
            beelinkHeaderLabel.Text = "SSH Beelink ▶";

            // Сворачиваем веб-сервер
            webServerContentPanel.Visible = false;
            webServerCollapsiblePanel.Height = webServerHeaderPanel.Height;
            webServerHeaderLabel.Text = "Веб-сервер ▶";

            // Сворачиваем демо-сценарии
            demoContentPanel.Visible = false;
            demoCollapsiblePanel.Height = demoHeaderPanel.Height;
            demoHeaderLabel.Text = "Демо-сценарии ▶";

            // Обновляем layout
            UpdateLayout();
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            // Обновляем расположение при изменении размера формы
            if (mainTabControl.SelectedTab == mainTabPage)
            {
                UpdateLayout();
            }
        }
        #endregion

        #region Обработчики сворачиваемых панелей
        private void CollapsiblePanelHeader_Click(object sender, EventArgs e)
        {
            Panel headerPanel;

            if (sender is Panel panel)
            {
                headerPanel = panel;
            }
            else if (sender is Label label)
            {
                headerPanel = label.Parent as Panel ?? new Panel();
            }
            else
            {
                return;
            }

            ToggleCollapsiblePanel(headerPanel);
        }

        private void ToggleCollapsiblePanel(Panel headerPanel)
        {
            var collapsiblePanel = headerPanel.Parent as Panel;
            if (collapsiblePanel == null || !_originalHeights.ContainsKey(collapsiblePanel))
                return;

            var contentPanel = collapsiblePanel.Controls.OfType<Panel>()
                .FirstOrDefault(p => p != headerPanel);

            if (contentPanel == null)
                return;

            if (contentPanel.Visible)
            {
                // Сворачиваем
                contentPanel.Visible = false;
                collapsiblePanel.Height = headerPanel.Height;

                // Обновляем текст заголовка
                UpdateHeaderLabel(headerPanel, false);
            }
            else
            {
                // Разворачиваем
                contentPanel.Visible = true;
                collapsiblePanel.Height = _originalHeights[collapsiblePanel];

                // Обновляем текст заголовка
                UpdateHeaderLabel(headerPanel, true);
            }

            // Обновляем layout
            UpdateLayout();
        }

        private void UpdateHeaderLabel(Panel headerPanel, bool isExpanded)
        {
            var label = headerPanel.Controls.OfType<Label>().FirstOrDefault();
            if (label == null) return;

            var collapsiblePanel = headerPanel.Parent as Panel;
            var panelType = collapsiblePanel?.Tag?.ToString() ?? "";

            switch (panelType)
            {
                case "beelink":
                    label.Text = isExpanded ? "SSH Beelink ▼" : "SSH Beelink ▶";
                    break;
                case "webserver":
                    label.Text = isExpanded ? "Веб-сервер ▼" : "Веб-сервер ▶";
                    break;
                case "demo":
                    label.Text = isExpanded ? "Демо-сценарии ▼" : "Демо-сценарии ▶";
                    break;
            }
        }

        private void UpdateLayout()
        {
            // Обновляем расположение всех панелей
            leftFlowLayout?.PerformLayout();
            panelRight?.PerformLayout();
        }
        #endregion

        #region Обработчики SSH подключения
        private async void beelinkConnectButton_Click(object sender, EventArgs e)
        {
            if (_sshManager == null)
            {
                MessageBox.Show("Менеджер подключения не инициализирован",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var config = ConfigService.Config.SSHBeelink;
            await _sshManager.ConnectAsync(config);
        }

        private void beelinkDisconnectButton_Click(object sender, EventArgs e)
        {
            _sshManager?.Disconnect();
        }

        private void beelinkTestButton_Click(object sender, EventArgs e)
        {
            if (_sshManager?.IsConnected() == true)
            {
                _sshManager.TestConnection();
            }
            else
            {
                beelinkConnectButton_Click(sender, e);
            }
        }
        #endregion

        #region Обработчики команд времени
        private void timeCheckButton_Click(object sender, EventArgs e)
        {
            _timeManager?.CheckTimeStatus();
        }

        private void timeEnableNTPButton_Click(object sender, EventArgs e)
        {
            _timeManager?.EnableNTP();
        }

        private void timeDisableNTPButton_Click(object sender, EventArgs e)
        {
            _timeManager?.DisableNTP();
        }

        private void timeSetButton_Click(object sender, EventArgs e)
        {
            _timeManager?.SetManualDateTime(manualDatePicker.Value, manualTimePicker.Value);
        }
        #endregion

        #region Обработчики UI
        private void beelinkClearLogButton_Click(object sender, EventArgs e)
        {
            _logManager?.ClearLog();
        }

        private void beelinkLogTextBox_TextChanged(object sender, EventArgs e)
        {
            beelinkLogTextBox.SelectionStart = beelinkLogTextBox.Text.Length;
            beelinkLogTextBox.ScrollToCaret();
        }
        #endregion

        #region Обработчики настроек
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_settingsManager.SaveSshSettings(_logManager))
            {
                MessageBox.Show("Настройки SSH Beelink сохранены!",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Сбросить настройки SSH Beelink к значениям по умолчанию?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _settingsManager.ResetSshSettings(_logManager);
            }
        }
        #endregion

        #region Обработчики настроек веб-сервера
        private void webServerSaveButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_settingsManager.SaveWebServerSettings(_logManager))
            {
                MessageBox.Show("Настройки веб-сервера сохранены!",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void webServerResetButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Сбросить настройки веб-сервера к значениям по умолчанию?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _settingsManager.ResetWebServerSettings(_logManager);
            }
        }
        #endregion

        #region Обработчики веб-сервера
        private void WebServerStartButton_Click(object sender, EventArgs e)
        {
            if (_webServerManager == null)
            {
                MessageBox.Show("Менеджер веб-сервера не инициализирован",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var config = ConfigService.Config.WebServer;
            _webServerManager.StartServer(config);
        }

        private void WebServerStopButton_Click(object sender, EventArgs e)
        {
            _webServerManager?.StopServer();
        }
        #endregion

        #region События формы
        private void MainForm_Load(object sender, EventArgs e)
        {
            _logManager?.AppendLog("Приложение запущено");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _sshService?.Dispose();
                _webServerService?.Dispose();
                _demoManager?.Cleanup();
                _logManager?.AppendLog("Приложение закрывается");
            }
            catch (Exception ex)
            {
                _logManager?.AppendLog($"Ошибка при закрытии: {ex.Message}");
            }
        }

        private void flowLayoutPanel_Paint(object sender, PaintEventArgs e)
        {
            // Пустая реализация для обработки события
        }

        private void webServerContentPanel_Paint(object sender, PaintEventArgs e)
        {
            // Пустая реализация для обработки события
        }
        #endregion

        private void demoContentPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainerMain_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}