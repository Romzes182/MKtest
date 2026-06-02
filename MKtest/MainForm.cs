using MKtest.Configs;
using MKtest.Managers;
using MKtest.Services;
using MKtest.Services.Demoscripts;
using MKtest.Services.HermesPassenger;
using MKtest.Services.USRTransfer;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MKtest.Services.JSONRPCprotokol;
using MKtest.Services.HTTPpay;
using MKtest.Services.SekopProtocol;

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
        private JSONRPCprotokolManager? _httpProtokolManager;
        private HTTPpayManager? _httpPayManager;
        private SekopProtocolManager? _sekopProtocolManager;
        private EmergencyManager? _emergencyManager;
        #endregion

        #region Сервисы (объявляем как nullable)
        private SSHService? _sshService;
        private TimeCommandsService? _timeService;
        private WebServerService? _webServerService;
        private DemoFileService? _demoFileService;
        private DemoScenarioService? _demoScenarioService;
        private JSONRPCprotokolService? _httpProtokolService;
        private HTTPpayService? _httpPayService;
        private IRouteService? _routeService;
        private IUSRTransferService? _usrTransferService;
        private SekopPacketSenderService? _sekopPacketSenderService;
        private SekopProtocolService? _sekopProtocolService;
        private EmergencyService? _emergencyService;
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
            _httpProtokolService = new JSONRPCprotokolService(ConfigService.Config.HTTPprotokol);
            _httpPayService = new HTTPpayService(ConfigService.Config.HTTPpay);
            _sekopPacketSenderService = new SekopPacketSenderService();
            _sekopProtocolService = new SekopProtocolService(ConfigService.Config.SekopProtocol,_sekopPacketSenderService);
            _emergencyService = new EmergencyService(_logManager);

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
                webServerIpTextBox, webServerPortNumeric, usrTransferIpTextBox, usrTransferPortNumeric, hermesIpTextBox, hermesPortNumeric,
                hermesUserTextBox, hermesPasswordTextBox, httpProtokolIpTextBox, httpProtokolPortNumeric, httpPayIpTextBox, httpPayPortNumeric,
                httpPayTerminalTextBox, httpPayRouteTextBox, httpPayTripNumeric, httpPayTripDatePicker, httpPayCurrentNumeric, httpPayIntervalNumeric,
                sekopIpTextBox, sekopPortNumeric
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
            if (_sekopProtocolService == null || _logManager == null)
                throw new InvalidOperationException("Сервисы протокола СЭКОП не инициализированы");
            _sekopProtocolManager = new SekopProtocolManager(_sekopProtocolService,_logManager);

            if (_emergencyService == null)
                throw new InvalidOperationException("EmergencyService не инициализирован");
            _emergencyManager = new EmergencyManager(_emergencyService,ConfigService.Config.Emergency);
            InitializeSekopProtocolMain();

            InitializeDemoServices();

            if (_routeService == null || _usrTransferService == null || _logManager == null)
                throw new InvalidOperationException("USR сервисы не инициализированы");
            _usrTransferManager = new USRTransferManager(_routeService, _usrTransferService, _logManager);
            InitializeUsrTransferUi();
            InitializeHermesMain();
            InitializeHTTPpayMain();
            InitializeHTTPprotokol();
        }

        private void InitializeDemoServices()
        {
            try
            {
                var demoConfig = new DemoConfig();
                _demoFileService = new DemoFileService(demoConfig);
                _demoScenarioService = new DemoScenarioService(_demoFileService, _logManager!);
                _demoManager = new DemoScenarioManager(cmbDemoScenarios, btnStartDemo, btnStopDemo,
                    lblDemoStatus, beelinkLogTextBox, _demoScenarioService);
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


            hermesCollapsiblePanel.Tag = "hermes";

            httpPayHeaderPanel.Click += CollapsiblePanelHeader_Click;
            httpPayHeaderLabel.Click += CollapsiblePanelHeader_Click;
            httpPayCollapsiblePanel.Tag = "httppay";

            httpProtokolHeaderPanel.Click += CollapsiblePanelHeader_Click;
            httpProtokolHeaderLabel.Click += CollapsiblePanelHeader_Click;
            httpProtokolCollapsiblePanel.Tag = "httpprotokol";

            btnHttpProtokolStart.Click += btnHttpProtokolStart_Click;
            btnHttpProtokolStop.Click += btnHttpProtokolStop_Click;
            btnHttpProtokolTest.Click += btnHttpProtokolTest_Click;
            btnHttpProtokolUpdate.Click += btnHttpProtokolUpdate_Click;


            sekopHeaderPanel.Click -= CollapsiblePanelHeader_Click;
            sekopHeaderLabel.Click -= CollapsiblePanelHeader_Click;

            sekopHeaderPanel.Click += CollapsiblePanelHeader_Click;
            sekopHeaderLabel.Click += CollapsiblePanelHeader_Click;

            sekopCollapsiblePanel.Tag = "sekop";
            sekopTransactionsNumeric.ValueChanged += sekopValuesNumeric_ValueChanged;
            sekopPassengersNumeric.ValueChanged += sekopValuesNumeric_ValueChanged;

            emergencyHeaderPanel.Click += CollapsiblePanelHeader_Click;
            emergencyHeaderLabel.Click += CollapsiblePanelHeader_Click;
            emergencyCollapsiblePanel.Tag = "emergency";


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
            _originalHeights[hermesCollapsiblePanel] = hermesCollapsiblePanel.Height;
            _originalHeights[httpPayCollapsiblePanel] = httpPayCollapsiblePanel.Height;
            _originalHeights[httpProtokolCollapsiblePanel] = httpProtokolCollapsiblePanel.Height;
            _originalHeights[sekopCollapsiblePanel] = sekopCollapsiblePanel.Height;
            _originalHeights[emergencyCollapsiblePanel] = emergencyCollapsiblePanel.Height;
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
            demoHeaderLabel.Text = "ЛК-ВИЗ через route.json ▶";

            usrTransferContentPanel.Visible = false;
            usrTransferCollapsiblePanel.Height = usrTransferHeaderPanel.Height;
            usrTransferHeaderLabel.Text = "ИР-0652 ▶";

            hermesContentPanel.Visible = false;
            hermesCollapsiblePanel.Height = hermesHeaderPanel.Height;
            hermesHeaderLabel.Text = "Гермес ▶";

            httpPayContentPanel.Visible = false;
            httpPayCollapsiblePanel.Height = httpPayHeaderPanel.Height;
            httpPayHeaderLabel.Text = "Протокол HTTP ▶";

            httpProtokolContentPanel.Visible = false;
            httpProtokolCollapsiblePanel.Height = httpProtokolHeaderPanel.Height;
            httpProtokolHeaderLabel.Text = "Протокол JSON-RPC ▶";

            sekopContentPanel.Visible = false;
            sekopCollapsiblePanel.Height = sekopHeaderPanel.Height;
            sekopHeaderLabel.Text = "Протокол СЭКОП ▶";

            emergencyContentPanel.Visible = false;
            emergencyCollapsiblePanel.Height = emergencyHeaderPanel.Height;
            emergencyHeaderLabel.Text ="Команды МЧС ▶";
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
                    label.Text = isExpanded ? "ЛК-ВИЗ через route.json ▼" : "ЛК-ВИЗ через route.json ▶";
                    break;

                case "usrtransfer":
                    label.Text = isExpanded ? "ИР-0652 ▼" : "ИР-0652 ▶";
                    break;
                case "hermes":
                    label.Text = isExpanded ? "Гермес ▼" : "Гермес ▶";
                    break;
                case "httppay":
                    label.Text = isExpanded ? "Протокол HTTP ▼" : "Протокол HTTP ▶";
                    break;
                case "httpprotokol":
                    label.Text = isExpanded
                        ? "Протокол JSON-RPC ▼" : "Протокол JSON-RPC ▶";
                    break;
                case "sekop":
                    label.Text = isExpanded
                        ? "Протокол СЭКОП ▼" : "Протокол СЭКОП ▶";
                    break;
                case "emergency":
                    label.Text = isExpanded
                        ? "Команды МЧС ▼": "Команды МЧС ▶";
                    break;
            }
        }

        private void UpdateLayout()
        {
            leftFlowLayout?.PerformLayout();
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

        #region Hermes Main

        private HermesPassengerManager? _hermesManager;
        private IHermesPassengerService? _hermesService;

        private void InitializeHermesMain()
        {
            if (_logManager == null) return;

            var cfg = ConfigService.Config.HermesSSH;
            _hermesService = new HermesPassengerService(_logManager, cfg);
            _hermesManager = new HermesPassengerManager(_hermesService, _logManager);
            _hermesManager.StatusChanged += HermesManager_StatusChanged;

            hermesEnteredTextBox.Text = "0";
            hermesExitedTextBox.Text = "1";
            lblHermesStatus.Text = "Статус: Остановлено";
            SetHermesButtons(false);
        }

        private async void btnHermesStart_Click(object sender, EventArgs e)
        {
            if (_hermesManager == null || _logManager == null) return;
            if (!TryReadHermesValues(out var entered, out var exited)) return;
            if (_hermesManager.IsRunning) return;

            SetHermesButtons(true);
            lblHermesStatus.Text = "Статус: Запуск...";

            try
            {
                await _hermesManager.StartAsync(entered, exited);
            }
            catch (Exception ex)
            {
                _logManager.AppendLog($"Hermes start error: {ex.Message}");
                lblHermesStatus.Text = "Статус: Ошибка подключения";
                MessageBox.Show($"Hermes недоступен:\n{ex.Message}", "Hermes",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (_hermesManager.IsRunning == false)
                    SetHermesButtons(false);
            }
        }

        private async void btnHermesStop_Click(object sender, EventArgs e)
        {
            if (_hermesManager == null || _logManager == null) return;

            try
            {
                await _hermesManager.StopAsync();
            }
            catch (Exception ex)
            {
                _logManager.AppendLog($"Hermes stop error: {ex.Message}");
            }
            finally
            {
                SetHermesButtons(false);
                lblHermesStatus.Text = "Статус: Остановлено";
            }
        }

        private async void btnHermesTest_Click(object sender, EventArgs e)
        {
            if (_hermesManager == null || _logManager == null) return;

            try
            {
                lblHermesStatus.Text = "Статус: Тест...";
                var ok = await _hermesManager.TestConnectionAsync();
                lblHermesStatus.Text = ok ? "Статус: Доступно" : "Статус: Недоступно";
                _logManager.AppendLog(ok
                    ? "Hermes SSH: подключение успешно"
                    : "Hermes SSH: подключение не удалось");
            }
            catch (Exception ex)
            {
                _logManager.AppendLog($"Hermes test fatal: {ex.Message}");
                lblHermesStatus.Text = "Статус: Ошибка";
            }
        }

        private void btnHermesUpdate_Click(object sender, EventArgs e)
        {
            if (_hermesManager == null || _logManager == null) return;
            if (!TryReadHermesValues(out var entered, out var exited)) return;

            _hermesManager.UpdateValues(entered, exited);
            _logManager.AppendLog($"Hermes: обновлены значения A={entered}, B={exited}");
        }

        private bool TryReadHermesValues(out int entered, out int exited)
        {
            entered = 0;
            exited = 0;

            if (!int.TryParse(hermesEnteredTextBox.Text, out entered) || entered < 0 || entered > 255)
            {
                MessageBox.Show("Поле 'Вошло' должно быть числом 0..255", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(hermesExitedTextBox.Text, out exited) || exited < 0 || exited > 255)
            {
                MessageBox.Show("Поле 'Вышло' должно быть числом 0..255", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void HermesManager_StatusChanged(string status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => HermesManager_StatusChanged(status)));
                return;
            }

            lblHermesStatus.Text = $"Статус: {status}";
            SetHermesButtons(_hermesManager?.IsRunning == true);
        }

        private void SetHermesButtons(bool running)
        {
            btnHermesStart.Enabled = !running;
            btnHermesStop.Enabled = running;
            btnHermesTest.Enabled = !running;
            btnHermesUpdate.Enabled = true;
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
                _httpPayManager?.Stop();
                _httpPayService?.Dispose();
                _demoManager?.Cleanup();
                _logManager?.AppendLog("Приложение закрывается");
                if (_hermesManager != null)
                    _hermesManager.StopAsync().Wait(1500);
            }
            catch (Exception ex)
            {
                _logManager?.AppendLog($"Ошибка при закрытии: {ex.Message}");

            }
        }

        private void flowLayoutPanel_Paint(object sender, PaintEventArgs e) { }
        private void webServerContentPanel_Paint(object sender, PaintEventArgs e) { }
        #endregion



        private void hermesSaveButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_settingsManager.SaveHermesSettings(_logManager))
            {
                MessageBox.Show("Настройки Hermes сохранены!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void hermesResetButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Сбросить настройки Hermes к значениям по умолчанию?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _settingsManager.ResetHermesSettings(_logManager);
            }
        }

        #region JSONRPCprotokol

        private void InitializeHTTPprotokol()
        {
            if (_httpProtokolService == null || _logManager == null)
                return;
            _httpProtokolManager = new JSONRPCprotokolManager(_httpProtokolService, _logManager);
            httpProtokolTrCounterTextBox.Text = "0";
            httpProtokolIntervalTextBox.Text = "5";
            lblHttpStatus.Text = "Статус: Остановлено";
            btnHttpProtokolStart.Enabled = true;
            btnHttpProtokolStop.Enabled = false;
        }

        private void httpProtokolSaveButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_settingsManager.SaveSettings(_logManager))
            {
                MessageBox.Show("Настройки HTTP Protokol сохранены!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void httpProtokolResetButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Сбросить настройки HTTP Protokol к значениям по умолчанию?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _settingsManager.ResetHTTPProtokolSettings(_logManager);
            }
        }

        private void btnHttpProtokolStart_Click(object sender, EventArgs e)
        {
            if (_httpProtokolManager == null)
                return;
            try
            {
                int trCounter = int.Parse(httpProtokolTrCounterTextBox.Text);
                int interval = int.Parse(httpProtokolIntervalTextBox.Text);
                _httpProtokolManager.Start(trCounter, interval);
                btnHttpProtokolStart.Enabled = false;
                btnHttpProtokolStop.Enabled = true;
                lblHttpStatus.Text = "Статус: Запущено"; _logManager?.AppendLog("HTTPprotokol запущен");
            }
            catch (Exception ex)
            {
                _logManager?.AppendLog(ex.Message);
            }
        }

        private void btnHttpProtokolStop_Click(object sender, EventArgs e)
        {
            if (_httpProtokolManager == null)
                return;
            _httpProtokolManager.Stop();
            btnHttpProtokolStart.Enabled = true;
            btnHttpProtokolStop.Enabled = false;
            lblHttpStatus.Text = "Статус: Остановлено";
            _logManager?.AppendLog("HTTPprotokol остановлен");
        }
        private async void btnHttpProtokolTest_Click(object sender, EventArgs e)
        {
            if (_httpProtokolService == null)
                return;
            try
            {
                int trCounter = int.Parse(httpProtokolTrCounterTextBox.Text);
                await _httpProtokolService.SendAsync(trCounter, CancellationToken.None);
                _logManager?.AppendLog("HTTPprotokol тест отправлен");
            }
            catch (Exception ex)
            {
                _logManager?.AppendLog(ex.Message);
            }
        }
        private void btnHttpProtokolUpdate_Click(object sender, EventArgs e)
        {
            if (_httpProtokolManager == null)
                return;

            try
            {
                int trCounter =
                    int.Parse(httpProtokolTrCounterTextBox.Text);

                int interval =
                    int.Parse(httpProtokolIntervalTextBox.Text);

                _httpProtokolManager.UpdateValues(
                    trCounter,
                    interval);

                _logManager?.AppendLog(
                    "JSONRPCprotokol параметры обновлены");
            }
            catch (Exception ex)
            {
                _logManager?.AppendLog(ex.Message);
            }
        }
        #endregion

        #region HTTPpay Main

        private void InitializeHTTPpayMain()
        {
            if (_httpPayService == null || _logManager == null)
                return;

            _httpPayManager =
                new HTTPpayManager(
                    _httpPayService,
                    _logManager);

            httpPayTotalNumeric.Value =
                Math.Min(
                    httpPayTotalNumeric.Maximum,
                    (decimal)ConfigService.Config.HTTPpay.TotalPayments);

            lblHttpPayStatus.Text = "Статус: Остановлено";
            SetHttpPayButtons(false);
        }

        private void btnHttpPayStart_Click(object sender, EventArgs e)
        {
            if (_httpPayManager == null || _logManager == null)
                return;

            if (_httpPayManager.IsRunning)
                return;

            var intervalSeconds =
                ConfigService.Config.HTTPpay.IntervalSeconds;

            // ВАЖНО:
            // передаём метод чтения, а не готовое число.
            // Поэтому pTotal читается заново перед каждой отправкой.
            _httpPayManager.Start(
                ReadHttpPayTotal,
                intervalSeconds);

            lblHttpPayStatus.Text = "Статус: Запущено";
            SetHttpPayButtons(true);

            _logManager.AppendLog(
                $"HTTPpay запущен, интервал {intervalSeconds} сек");
        }

        private void btnHttpPayStop_Click(object sender, EventArgs e)
        {
            _httpPayManager?.Stop();

            lblHttpPayStatus.Text = "Статус: Остановлено";
            SetHttpPayButtons(false);
        }

        private int ReadHttpPayTotal()
        {
            if (httpPayTotalNumeric.InvokeRequired)
            {
                return (int)httpPayTotalNumeric.Invoke(
                    new Func<int>(() => (int)httpPayTotalNumeric.Value));
            }

            return (int)httpPayTotalNumeric.Value;
        }

        private void SetHttpPayButtons(bool running)
        {
            btnHttpPayStart.Enabled = !running;
            btnHttpPayStop.Enabled = running;

            // ВАЖНО:
            // pTotal не отключаем во время работы,
            // чтобы его можно было менять на лету.
            httpPayTotalNumeric.Enabled = true;
        }

        #endregion

        private void httpPaySaveButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (_settingsManager.SaveHTTPpaySettings(_logManager))
            {
                MessageBox.Show("Настройки HTTP протокола сохранены!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
        private void httpPayResetButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("Сбросить настройки HTTP протокола к значениям по умолчанию?",
              "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _settingsManager.ResetHTTPpaySettings(_logManager);
            }

        }

        #region Сэкоп Протокол

        private SekopValues ReadSekopValues()
        {
            if (sekopTransactionsNumeric.InvokeRequired ||
                sekopPassengersNumeric.InvokeRequired)
            {
                return (SekopValues)Invoke(
                    new Func<SekopValues>(ReadSekopValues));
            }

            return new SekopValues(
                (int)sekopTransactionsNumeric.Value,
                (int)sekopPassengersNumeric.Value);
        }
        private void InitializeSekopProtocolMain()
        {
            sekopTransactionsNumeric.Value = 0;
            sekopPassengersNumeric.Value = 0;

            sekopStatusLabel.Text = "Статус: Остановлено";

            sekopStartButton.Enabled = true;
            sekopStopButton.Enabled = false;
        }
        private async void sekopStartButton_Click(object sender, EventArgs e)
        {
            if (_sekopProtocolManager == null)
                return;

            try
            {
                await _sekopProtocolManager.StartAsync(
                    ReadSekopValues);

                sekopStartButton.Enabled = false;
                sekopStopButton.Enabled = true;

                sekopStatusLabel.Text = "Статус: Запущено";
            }
            catch (Exception ex)
            {
                sekopStatusLabel.Text = "Статус: Ошибка";

                _logManager?.AppendLog(
                    $"Ошибка запуска протокола СЭКОП: {ex.Message}");
            }
        }

        private void sekopStopButton_Click(object sender, EventArgs e)
        {
            if (_sekopProtocolManager == null)
                return;

            _sekopProtocolManager.Stop();

            sekopStartButton.Enabled = true;
            sekopStopButton.Enabled = false;

            sekopStatusLabel.Text = "Статус: Остановлено";
        }



        private void sekopSaveButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show(
                    "Менеджеры не инициализированы",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            if (_settingsManager.SaveSekopProtocolSettings(_logManager))
            {
                MessageBox.Show(
                    "Настройки протокола СЭКОП сохранены!",
                    "Успех",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        private void sekopResetButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show(
                    "Менеджеры не инициализированы",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            if (MessageBox.Show(
                    "Сбросить настройки протокола СЭКОП к значениям по умолчанию?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _settingsManager.ResetSekopProtocolSettings(_logManager);
            }
        }

        private async void sekopValuesNumeric_ValueChanged(object? sender, EventArgs e)
        {
            if (_sekopProtocolManager?.IsRunning != true)
                return;

            try
            {
                await _sekopProtocolManager.SendCurrentAsync();
            }
            catch (Exception ex)
            {
                _logManager?.AppendLog(
                    $"Ошибка отправки СЭКОП при изменении значений: {ex.Message}");
            }
        }

        #endregion

        #region МЧС
        private async void btnEmergency1_Click(object sender,EventArgs e)
        {
            if (_emergencyManager == null)
                return;

            await _emergencyManager.SendCommand1Async();
        }

        private async void btnEmergency2_Click(object sender,EventArgs e)
        {
            if (_emergencyManager == null)
                return;

            await _emergencyManager.SendCommand2Async();
        }

        private async void btnEmergency3_Click(object sender,EventArgs e)
        {
            if (_emergencyManager == null)
                return;

            await _emergencyManager.SendCommand3Async();
        }
        private async void btnEmergency4_Click(object sender, EventArgs e)
        {
            if (_emergencyManager == null)
                return;

            await _emergencyManager.SendCommand4Async();
        }
        #endregion
    }
}