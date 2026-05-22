using MKtest.Configs;
using MKtest.Managers;
using MKtest.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using MKtest.Services.Demoscripts;
using MKtest.Services.USRTransfer;

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
        private USRTransferManager? _usrTransferManager;
        private IRouteService? _routeService;
        private IUSRTransferService? _usrTransferService;
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
                RepositionLogPanel();
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
            if (logPanel.Parent == mainTabPage) return;
            if (logPanel.Parent != null) logPanel.Parent.Controls.Remove(logPanel);
            mainTabPage.Controls.Add(logPanel);
            logPanel.Dock = DockStyle.Bottom;
            logPanel.Height = 200;
            logPanel.BringToFront();
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.BringToFront();
        }
        #endregion

        #region Инициализация сервисов
        private void InitializeServices()
        {
            _logManager = new LogManager(beelinkLogTextBox);
            _sshService = new SSHService();
            _timeService = new TimeCommandsService(_sshService);
            _webServerService = new WebServerService(_logManager);
            _routeService = new RouteService();
            _usrTransferService = new USRTransferService(_logManager);
        }
        #endregion

        #region Инициализация менеджеров
        private void InitializeManagers()
        {
            _stateManager = new ConnectionStateManager(
                beelinkConnectButton, beelinkDisconnectButton, beelinkTestButton, beelinkStatusLabel,
                timeCheckButton, timeEnableNTPButton, timeDisableNTPButton, timeSetButton,
                manualDatePicker, manualTimePicker, timeGroupBox
            );

            _webServerStateManager = new WebServerStateManager(
                webServerStartButton, webServerStopButton, webServerStatusLabel
            );

            _settingsManager = new SettingsManager(
                ipTextBox, portNumeric, userTextBox, passwordUserTextBox, passwordRootTextBox,
                webServerIpTextBox, webServerPortNumeric, usrTransferIpTextBox, usrTransferPortNumeric
            );

            if (_sshService == null || _logManager == null || _stateManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");
            _sshManager = new SSHConnectionManager(_sshService, _logManager, _stateManager);

            if (_webServerService == null || _logManager == null || _webServerStateManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");
            _webServerManager = new WebServerManager(_webServerService, _logManager, _webServerStateManager);

            if (_timeService == null || _logManager == null || _sshManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");
            _timeManager = new TimeCommandsManager(_timeService, _logManager, () => _sshManager.IsConnected());

            InitializeDemoServices();

            if (_routeService == null || _usrTransferService == null || _logManager == null)
                throw new InvalidOperationException("USR сервисы не инициализированы");
            _usrTransferManager = new USRTransferManager(_routeService, _usrTransferService, _logManager);
            InitializeUsrTransferUi();
        }

        private void InitializeDemoServices()
        {
            try
            {
                var demoConfig = new DemoConfig();
                _demoFileService = new DemoFileService(demoConfig);
                _demoScenarioService = new DemoScenarioService(_demoFileService, _logManager!);
                _demoManager = new DemoScenarioManager(
                    cmbDemoScenarios, btnStartDemo, btnStopDemo, lblDemoStatus, beelinkLogTextBox, _demoScenarioService
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
            manualDatePicker.Format = DateTimePickerFormat.Short;
            manualTimePicker.Format = DateTimePickerFormat.Time;
            manualTimePicker.ShowUpDown = true;

            SaveOriginalHeights();

            beelinkHeaderPanel.Click += CollapsiblePanelHeader_Click;
            beelinkHeaderLabel.Click += CollapsiblePanelHeader_Click;
            beelinkCollapsiblePanel.Tag = "beelink";

            webServerHeaderPanel.Click += CollapsiblePanelHeader_Click;
            webServerHeaderLabel.Click += CollapsiblePanelHeader_Click;
            webServerCollapsiblePanel.Tag = "webserver";

            demoHeaderPanel.Click += CollapsiblePanelHeader_Click;
            demoHeaderLabel.Click += CollapsiblePanelHeader_Click;
            demoCollapsiblePanel.Tag = "demo";

            usrTransferHeaderPanel.Click += CollapsiblePanelHeader_Click;
            usrTransferHeaderLabel.Click += CollapsiblePanelHeader_Click;
            usrTransferCollapsiblePanel.Tag = "usrtransfer";

            webServerStartButton.Click += WebServerStartButton_Click;
            webServerStopButton.Click += WebServerStopButton_Click;

            _settingsManager?.LoadSettings();
            Resize += MainForm_Resize;
            CollapseAllPanels();
        }

        private void SaveOriginalHeights()
        {
            _originalHeights[beelinkCollapsiblePanel] = beelinkCollapsiblePanel.Height;
            _originalHeights[webServerCollapsiblePanel] = webServerCollapsiblePanel.Height;
            _originalHeights[demoCollapsiblePanel] = demoCollapsiblePanel.Height;
            _originalHeights[usrTransferCollapsiblePanel] = usrTransferCollapsiblePanel.Height;
        }

        private void CollapseAllPanels()
        {
            beelinkContentPanel.Visible = false;
            beelinkCollapsiblePanel.Height = beelinkHeaderPanel.Height;
            beelinkHeaderLabel.Text = "SSH Beelink ▶";

            webServerContentPanel.Visible = false;
            webServerCollapsiblePanel.Height = webServerHeaderPanel.Height;
            webServerHeaderLabel.Text = "Веб-сервер ▶";

            demoContentPanel.Visible = false;
            demoCollapsiblePanel.Height = demoHeaderPanel.Height;
            demoHeaderLabel.Text = "Демо-сценарии ▶";

            usrTransferContentPanel.Visible = false;
            usrTransferCollapsiblePanel.Height = usrTransferHeaderPanel.Height;
            usrTransferHeaderLabel.Text = "ИР-0652 ▶";

            UpdateLayout();
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            if (mainTabControl.SelectedTab == mainTabPage) UpdateLayout();
        }
        #endregion

        #region Обработчики сворачиваемых панелей
        private void CollapsiblePanelHeader_Click(object sender, EventArgs e)
        {
            Panel? headerPanel = sender as Panel ?? (sender as Label)?.Parent as Panel;
            if (headerPanel == null) return;
            ToggleCollapsiblePanel(headerPanel);
        }

        private void ToggleCollapsiblePanel(Panel headerPanel)
        {
            var collapsiblePanel = headerPanel.Parent as Panel;
            if (collapsiblePanel == null || !_originalHeights.ContainsKey(collapsiblePanel)) return;

            var contentPanel = collapsiblePanel.Controls.OfType<Panel>().FirstOrDefault(p => p != headerPanel);
            if (contentPanel == null) return;

            if (contentPanel.Visible)
            {
                contentPanel.Visible = false;
                collapsiblePanel.Height = headerPanel.Height;
                UpdateHeaderLabel(headerPanel, false);
            }
            else
            {
                contentPanel.Visible = true;
                collapsiblePanel.Height = _originalHeights[collapsiblePanel];
                UpdateHeaderLabel(headerPanel, true);
            }

            UpdateLayout();
        }

        private void UpdateHeaderLabel(Panel headerPanel, bool isExpanded)
        {
            var label = headerPanel.Controls.OfType<Label>().FirstOrDefault();
            if (label == null) return;

            var panelType = (headerPanel.Parent as Panel)?.Tag?.ToString() ?? "";
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

                case "usrtransfer":
                    label.Text = isExpanded ? "ИР-0652 ▼" : "ИР-0652 ▶";
                    break;
            }
        }

        private void UpdateLayout()
        {
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

        private void beelinkDisconnectButton_Click(object sender, EventArgs e) => _sshManager?.Disconnect();

        private void beelinkTestButton_Click(object sender, EventArgs e)
        {
            if (_sshManager?.IsConnected() == true) _sshManager.TestConnection();
            else beelinkConnectButton_Click(sender, e);
        }
        #endregion

        #region Обработчики команд времени
        private void timeCheckButton_Click(object sender, EventArgs e) => _timeManager?.CheckTimeStatus();
        private void timeEnableNTPButton_Click(object sender, EventArgs e) => _timeManager?.EnableNTP();
        private void timeDisableNTPButton_Click(object sender, EventArgs e) => _timeManager?.DisableNTP();
        private void timeSetButton_Click(object sender, EventArgs e) => _timeManager?.SetManualDateTime(manualDatePicker.Value, manualTimePicker.Value);
        #endregion

        #region Обработчики UI
        private void beelinkClearLogButton_Click(object sender, EventArgs e) => _logManager?.ClearLog();

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
                MessageBox.Show("Менеджеры не инициализированы", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_settingsManager.SaveSshSettings(_logManager))
                MessageBox.Show("Настройки SSH Beelink сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Сбросить настройки SSH Beelink к значениям по умолчанию?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                _settingsManager.ResetSshSettings(_logManager);
        }
        #endregion

        #region Обработчики настроек веб-сервера
        private void webServerSaveButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_settingsManager.SaveWebServerSettings(_logManager))
                MessageBox.Show("Настройки веб-сервера сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void webServerResetButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Сбросить настройки веб-сервера к значениям по умолчанию?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                _settingsManager.ResetWebServerSettings(_logManager);
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

            _webServerManager.StartServer(ConfigService.Config.WebServer);
        }

        private void WebServerStopButton_Click(object sender, EventArgs e) => _webServerManager?.StopServer();
        #endregion

        #region USR Transfer
        private void InitializeUsrTransferUi()
        {
            if (_usrTransferManager == null) return;

            _usrTransferManager.ProgressChanged += UsrTransferManager_ProgressChanged;
            LoadUsrRoutesToUi();
            SetUsrButtons(false);

            if (lblUsrStatus != null) lblUsrStatus.Text = "Статус: Остановлен";
            if (lblUsrStep != null) lblUsrStep.Text = "Шаг: 0/0";
            if (lblUsrCountdown != null) lblUsrCountdown.Text = "След. через: -- сек";

            cmbInMode.Items.Clear();
            cmbInMode.Items.Add("Все подряд");
            cmbInMode.Items.Add("С выбранного");
            cmbInMode.Items.Add("Только выбранный");
            cmbInMode.SelectedIndex = 0;
        }

        private void LoadUsrRoutesToUi()
        {
            if (_usrTransferManager == null || cmbUsrRoutes == null) return;

            cmbUsrRoutes.Items.Clear();
            foreach (var route in _usrTransferManager.LoadRoutes())
                cmbUsrRoutes.Items.Add(route.RouteNumber);

            if (cmbUsrRoutes.Items.Count > 0) cmbUsrRoutes.SelectedIndex = 0;
        }

        private void cmbUsrRoutes_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_usrTransferManager == null || cmbUsrRoutes?.SelectedItem == null) return;

            var selected = cmbUsrRoutes.SelectedItem.ToString() ?? string.Empty;
            var route = _usrTransferManager.LoadRoutes().FirstOrDefault(r => r.RouteNumber == selected);

            lstSvcFiles?.Items.Clear();
            lstInFiles?.Items.Clear();
            if (route == null) return;

            foreach (var svc in route.SvcFilePaths) lstSvcFiles?.Items.Add(System.IO.Path.GetFileName(svc));
            foreach (var inf in route.InFilePaths) lstInFiles?.Items.Add(System.IO.Path.GetFileName(inf));

            if (lblUsrStatus != null) lblUsrStatus.Text = $"Статус: Маршрут {route.RouteNumber}";
            if (lblUsrStep != null) lblUsrStep.Text = $"Шаг: 0/{route.InFilePaths.Count}";
            if (lblUsrCountdown != null) lblUsrCountdown.Text = "След. через: -- сек";
        }

        private async void btnUsrTest_Click(object? sender, EventArgs e)
        {
            if (_usrTransferManager == null || _logManager == null) return;
            var ok = await _usrTransferManager.TestConnectionAsync();
            _logManager.AppendLog(ok ? "USR: подключение установлено" : "USR: тест не пройден (timeout/ошибка)");
        }

        private async void btnUsrSendSvc_Click(object? sender, EventArgs e)
        {
            if (_usrTransferManager == null || cmbUsrRoutes?.SelectedItem == null || lstSvcFiles?.SelectedItem == null)
            {
                _logManager?.AppendLog("Выберите маршрут и SVC файл");
                return;
            }

            var routeNumber = cmbUsrRoutes.SelectedItem.ToString() ?? string.Empty;
            var svcFileName = lstSvcFiles.SelectedItem.ToString() ?? string.Empty;
            await _usrTransferManager.SendSelectedSvcAsync(routeNumber, svcFileName);
        }

        private async void btnUsrStartIn_Click(object? sender, EventArgs e)
        {
            if (_usrTransferManager == null || cmbUsrRoutes?.SelectedItem == null) return;
            if (_usrTransferManager.IsSequenceRunning) return;

            var routeNumber = cmbUsrRoutes.SelectedItem.ToString() ?? string.Empty;
            var mode = cmbInMode?.SelectedItem?.ToString() ?? "Все подряд";
            var selectedIn = lstInFiles?.SelectedItem?.ToString() ?? string.Empty;

            SetUsrButtons(true);
            try
            {
                if (mode == "Все подряд")
                {
                    await _usrTransferManager.StartInSequenceAsync(routeNumber);
                }
                else if (mode == "С выбранного")
                {
                    if (string.IsNullOrWhiteSpace(selectedIn))
                    {
                        _logManager?.AppendLog("Выберите IN файл для режима 'С выбранного'");
                        return;
                    }
                    await _usrTransferManager.StartInFromSelectedAsync(routeNumber, selectedIn);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(selectedIn))
                    {
                        _logManager?.AppendLog("Выберите IN файл для режима 'Только выбранный'");
                        return;
                    }
                    await _usrTransferManager.SendSingleInAsync(routeNumber, selectedIn);
                }
            }
            finally
            {
                SetUsrButtons(false);
                if (lblUsrCountdown != null) lblUsrCountdown.Text = "След. через: -- сек";
            }
        }

        private void btnUsrStopIn_Click(object? sender, EventArgs e) => _usrTransferManager?.StopInSequence();

        private void UsrTransferManager_ProgressChanged(object? sender, TransferProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UsrTransferManager_ProgressChanged(sender, e)));
                return;
            }

            if (lblUsrStatus != null) lblUsrStatus.Text = $"Статус: {e.Status}";
            if (lblUsrStep != null) lblUsrStep.Text = $"Шаг: {e.CurrentStep}/{e.TotalSteps}";
            if (lblUsrCountdown != null)
                lblUsrCountdown.Text = e.CountdownSeconds > 0
                    ? $"След. через: {e.CountdownSeconds} сек"
                    : "След. через: -- сек";
        }

        private void SetUsrButtons(bool isRunning)
        {
            if (btnUsrStartIn != null) btnUsrStartIn.Enabled = !isRunning;
            if (btnUsrSendSvc != null) btnUsrSendSvc.Enabled = !isRunning;
            if (btnUsrStopIn != null) btnUsrStopIn.Enabled = isRunning;
            if (btnUsrTest != null) btnUsrTest.Enabled = !isRunning;
        }
        #endregion

        #region Обработчики настроек USR Transfer
        private void usrTransferSaveButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_settingsManager.SaveUSRTransferSettings(_logManager))
                MessageBox.Show("Настройки USR Transfer сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void usrTransferResetButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Сбросить настройки USR Transfer к значениям по умолчанию?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                _settingsManager.ResetUSRTransferSettings(_logManager);
        }
        #endregion

        #region События формы
        private void MainForm_Load(object sender, EventArgs e) => _logManager?.AppendLog("Приложение запущено");

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _usrTransferManager?.StopInSequence();
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

        private void flowLayoutPanel_Paint(object sender, PaintEventArgs e) { }
        private void webServerContentPanel_Paint(object sender, PaintEventArgs e) { }
        #endregion




        private void demoContentPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void demoHeaderLabel_Click(object sender, EventArgs e)
        {

        }

        private void panelRight_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}