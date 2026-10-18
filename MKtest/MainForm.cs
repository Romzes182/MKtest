using MKtest.Configs;
using MKtest.UI;
using MKtest.UI.USRTransfer;
using MKtest.Managers;
using MKtest.Managers.Settings;
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
using MKtest.UI.Sekop;
using MKtest.UI.Logging;
using MKtest.UI.JSONRPC;
using MKtest.UI.HTTPpay;
using MKtest.UI.Hermes;
using MKtest.UI.Emergency;
using MKtest.UI.Beelink;

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

        #region Контроллеры интерфейса
        private CollapsiblePanelController? _collapsiblePanelController;
        private DemoModeController? _demoModeController;
        private Button? _btnPreviousDemoStep;
        private USRTransferUiController? _usrTransferUiController;
        private SekopUiController? _sekopUiController;
        private JsonRpcUiController? _jsonRpcUiController;
        private HttpPayUiController? _httpPayUiController;
        private HermesUiController? _hermesUiController;
        private EmergencyUiController? _emergencyUiController;
        private BeelinkTimeUiController? _beelinkTimeUiController;
        private BlockLogPanelRegistry? _blockLogs;
        private RightPanelFlowController? _rightPanelLayout;
        private ScrollablePanelStackController? _leftPanelLayout;
        private bool _shutdownStarted;
        private bool _shutdownCompleted;
        private LogManager? _sshLog, _webLog, _demoLog, _usrLog, _hermesLog;
        private LogManager? _httpPayLog, _jsonLog, _sekopLog, _emergencyLog;
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
                SetupGeneralLogToolbar();
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
        /// <summary>
        /// A small toolbar for the shared log. The existing clear handler is
        /// retained, and the new button resumes following incoming entries.
        /// </summary>
        private void SetupGeneralLogToolbar()
        {
            var toolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 31,
                ColumnCount = 2,
                RowCount = 1,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));

            logPanel.Controls.Remove(beelinkClearLogButton);
            beelinkClearLogButton.Dock = DockStyle.Fill;
            beelinkClearLogButton.Margin = Padding.Empty;
            toolbar.Controls.Add(beelinkClearLogButton, 0, 0);

            var latestButton = new Button
            {
                Name = "btnResumeGeneralLog",
                Text = "Вниз ↓",
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                UseVisualStyleBackColor = true
            };
            latestButton.Click += (_, _) => _logManager?.ResumeLogScroll();
            toolbar.Controls.Add(latestButton, 1, 0);
            logPanel.Controls.Add(toolbar);
            toolbar.BringToFront();
        }
        #endregion

        #region Инициализация сервисов
        private void InitializeServices()
        {
            _logManager = new LogManager(beelinkLogTextBox);
            InitializeBlockLogs();
            _sshService = new SSHService();
            _timeService = new TimeCommandsService(_sshService);
            _webServerService = new WebServerService();
            _routeService = new RouteService();
            _usrTransferService = new USRTransferService(_usrLog!);
            _httpProtokolService = new JSONRPCprotokolService(ConfigService.Config.HTTPprotokol);
            _httpPayService = new HTTPpayService(ConfigService.Config.HTTPpay);
            _sekopPacketSenderService = new SekopPacketSenderService();
            _sekopProtocolService = new SekopProtocolService(ConfigService.Config.SekopProtocol,_sekopPacketSenderService);
            _emergencyService = new EmergencyService(_emergencyLog!);

        }
        #endregion

        /// <summary>Dedicated log inside each existing protocol block.</summary>
        private void InitializeBlockLogs()
        {
            if (_logManager == null) return;
            _blockLogs = new BlockLogPanelRegistry(_logManager);
            _sshLog = _blockLogs.Add("SSH Beelink", beelinkContentPanel, beelinkCollapsiblePanel);
            _webLog = _blockLogs.Add("Веб-сервер", webServerContentPanel, webServerCollapsiblePanel);
            _demoLog = _blockLogs.Add("ЛК-ВИЗ / Demo", demoContentPanel, demoCollapsiblePanel);
            _usrLog = _blockLogs.Add("ИР-0652", usrTransferContentPanel, usrTransferCollapsiblePanel);
            _hermesLog = _blockLogs.Add("Гермес", hermesContentPanel, hermesCollapsiblePanel);
            _httpPayLog = _blockLogs.Add("HTTP-платежи", httpPayContentPanel, httpPayCollapsiblePanel);
            _jsonLog = _blockLogs.Add("JSON-RPC", httpProtokolContentPanel, httpProtokolCollapsiblePanel);
            _sekopLog = _blockLogs.Add("СЭКОП", sekopContentPanel, sekopCollapsiblePanel);
            _emergencyLog = _blockLogs.Add("МЧС", emergencyContentPanel, emergencyCollapsiblePanel);
        }

        #region Инициализация менеджеров
        private void InitializeManagers()
        {
            _stateManager = new ConnectionStateManager(
                beelinkConnectButton, beelinkDisconnectButton, beelinkStatusLabel,
                timeCheckButton, timeEnableNTPButton, timeDisableNTPButton, timeSetButton,
                manualDatePicker, manualTimePicker, timeGroupBox
            );

            _webServerStateManager = new WebServerStateManager(
                webServerStartButton, webServerStopButton, webServerStatusLabel
            );

            var settingsStore = new ConfigServiceSettingsStore();
            _settingsManager = new SettingsManager(
                settingsStore,
                new SshSettingsSection(settingsStore,
                    ipTextBox, portNumeric, userTextBox, passwordUserTextBox, passwordRootTextBox),
                new WebServerSettingsSection(settingsStore,
                    webServerIpTextBox, webServerPortNumeric),
                new UsrTransferSettingsSection(settingsStore,
                    usrTransferIpTextBox, usrTransferPortNumeric),
                new HermesSettingsSection(
                    hermesIpTextBox, hermesPortNumeric, hermesUserTextBox, hermesPasswordTextBox),
                new JsonRpcSettingsSection(settingsStore,
                    httpProtokolIpTextBox, httpProtokolPortNumeric),
                new HttpPaySettingsSection(settingsStore,
                    httpPayIpTextBox, httpPayPortNumeric, httpPayTerminalTextBox, httpPayRouteTextBox,
                    httpPayTripNumeric, httpPayTripDatePicker, httpPayCurrentNumeric, httpPayIntervalNumeric),
                new SekopSettingsSection(settingsStore,
                    sekopIpTextBox, sekopPortNumeric));

            if (_sshService == null || _logManager == null || _stateManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");
            _sshManager = new SSHConnectionManager(_sshService, _sshLog!, _stateManager);

            if (_webServerService == null || _logManager == null || _webServerStateManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");
            _webServerManager = new WebServerManager(_webServerService, _webLog!, _webServerStateManager);

            if (_timeService == null || _logManager == null || _sshManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");
            _timeManager = new TimeCommandsManager(_timeService, _sshLog!, () => _sshManager.IsConnected());
            _beelinkTimeUiController = new BeelinkTimeUiController(
                _timeManager,
                () => _sshManager?.IsConnected() == true,
                () => _shutdownStarted,
                timeGroupBox,
                beelinkDisconnectButton,
                beelinkRemoteTimeTextBox,
                message => _sshLog?.AppendLog(message));
            if (_sekopProtocolService == null || _logManager == null)
                throw new InvalidOperationException("Сервисы протокола СЭКОП не инициализированы");
            _sekopProtocolManager = new SekopProtocolManager(_sekopProtocolService,_sekopLog!);

            if (_emergencyService == null)
                throw new InvalidOperationException("EmergencyService не инициализирован");
            _emergencyManager = new EmergencyManager(_emergencyService,ConfigService.Config.Emergency);
            _emergencyUiController = new EmergencyUiController(_emergencyManager, _emergencyLog!.AppendLog);
            InitializeSekopProtocolMain();

            InitializeDemoServices();

            if (_routeService == null || _usrTransferService == null || _logManager == null)
                throw new InvalidOperationException("USR сервисы не инициализированы");
            _usrTransferManager = new USRTransferManager(_routeService, _usrTransferService, _usrLog!);
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
                _demoScenarioService = new DemoScenarioService(_demoFileService, _demoLog!);
                _demoManager = new DemoScenarioManager(cmbDemoScenarios, btnStartDemo, btnStopDemo,
                    lblDemoStatus, _demoLog!, _demoScenarioService);
                _demoLog?.AppendLog("Демо-сервисы инициализированы");
            }
            catch (Exception ex)
            {
                _demoLog?.AppendLog($"Ошибка инициализации демо-сервисов: {ex.Message}");
            }
        }

      
        #endregion

        #region Настройка UI
        private void SetupUI()
        {
            manualDatePicker.Format = DateTimePickerFormat.Short;
            manualTimePicker.Format = DateTimePickerFormat.Time;
            manualTimePicker.ShowUpDown = true;

            // Both columns use a real scroll viewport with explicit content height.
            // The old FlowLayoutPanel stays in Designer but is not used at runtime.
            leftFlowLayout.Visible = false;
            panelLeft.Dock = DockStyle.Fill;
            panelDownRight.Dock = DockStyle.Fill;

            _leftPanelLayout = new ScrollablePanelStackController(
                panelLeft, webServerCollapsiblePanel, demoCollapsiblePanel,
                usrTransferCollapsiblePanel, emergencyCollapsiblePanel);
            _rightPanelLayout = new RightPanelFlowController(
                panelDownRight, httpProtokolCollapsiblePanel, httpPayCollapsiblePanel,
                sekopCollapsiblePanel, hermesCollapsiblePanel, beelinkCollapsiblePanel);
            _collapsiblePanelController = new CollapsiblePanelController(UpdateLayout);
            _collapsiblePanelController.Register(beelinkCollapsiblePanel, beelinkHeaderPanel, beelinkHeaderLabel, beelinkContentPanel, "SSH Beelink");
            _collapsiblePanelController.Register(webServerCollapsiblePanel, webServerHeaderPanel, webServerHeaderLabel, webServerContentPanel, "Веб-сервер");
            _collapsiblePanelController.Register(demoCollapsiblePanel, demoHeaderPanel, demoHeaderLabel, demoContentPanel, "ЛК-ВИЗ через route.json");
            _collapsiblePanelController.Register(usrTransferCollapsiblePanel, usrTransferHeaderPanel, usrTransferHeaderLabel, usrTransferContentPanel, "ИР-0652");
            _collapsiblePanelController.Register(hermesCollapsiblePanel, hermesHeaderPanel, hermesHeaderLabel, hermesContentPanel, "Гермес");
            _collapsiblePanelController.Register(httpPayCollapsiblePanel, httpPayHeaderPanel, httpPayHeaderLabel, httpPayContentPanel, "Протокол HTTP");
            _collapsiblePanelController.Register(httpProtokolCollapsiblePanel, httpProtokolHeaderPanel, httpProtokolHeaderLabel, httpProtokolContentPanel, "Протокол JSON-RPC");

            // В Designer имена заголовка СЭКОП исторически переставлены:
            // sekopHeaderLabel — Panel, sekopHeaderPanel — Label.
            _collapsiblePanelController.Register(sekopCollapsiblePanel, sekopHeaderLabel, sekopHeaderPanel, sekopContentPanel, "Протокол СЭКОП");
            _collapsiblePanelController.Register(emergencyCollapsiblePanel, emergencyHeaderPanel, emergencyHeaderLabel, emergencyContentPanel, "Команды МЧС");

            if (_blockLogs != null)
                _blockLogs.LogHeightChanged += _collapsiblePanelController.AdjustExpandedHeight;

            webServerStartButton.Click += WebServerStartButton_Click;
            webServerStopButton.Click += WebServerStopButton_Click;

            _settingsManager?.LoadSettings();
            Resize += MainForm_Resize;
            _collapsiblePanelController.CollapseAll();

            if (_demoScenarioService != null)
            {
                // The previous-step button is kept outside Designer.cs to avoid
                // generated-code conflicts and is visible only in manual mode.
                _btnPreviousDemoStep = new Button
                {
                    Name = "btnPreviousDemoStep",
                    Text = "Назад",
                    Location = new System.Drawing.Point(246, 37),
                    Size = new System.Drawing.Size(71, 25),
                    UseVisualStyleBackColor = true,
                    TabIndex = btnNextStep.TabIndex + 1
                };
                demoContentPanel.Controls.Add(_btnPreviousDemoStep);

                _demoModeController = new DemoModeController(
                    rbAutoMode, rbManualMode, btnStartDemo, btnNextStep,
                    _btnPreviousDemoStep, cmbDemoScenarios, _demoScenarioService,
                    message => _demoLog?.AppendLog(message));
            }
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            if (mainTabControl.SelectedTab == mainTabPage) UpdateLayout();
        }

        private void UpdateLayout()
        {
            _leftPanelLayout?.RefreshLayout();
            _rightPanelLayout?.RefreshLayout();
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

            _beelinkTimeUiController?.ClearReadout();
            var config = ConfigService.Config.SSHBeelink;
            await _sshManager.ConnectAsync(config);
        }

        private void beelinkDisconnectButton_Click(object sender, EventArgs e)
        {
            _sshManager?.Disconnect();
            _beelinkTimeUiController?.ClearReadout();
        }
        #endregion

        #region Обработчики команд времени
        // WinForms Designer handlers remain stable. Behavior lives in the UI controller.
        private async void timeCheckButton_Click(object sender, EventArgs e)
        {
            if (_beelinkTimeUiController != null)
                await _beelinkTimeUiController.CheckAsync();
        }

        private async void timeEnableNTPButton_Click(object sender, EventArgs e)
        {
            if (_beelinkTimeUiController != null)
                await _beelinkTimeUiController.EnableNtpAsync();
        }

        private async void timeDisableNTPButton_Click(object sender, EventArgs e)
        {
            if (_beelinkTimeUiController != null)
                await _beelinkTimeUiController.DisableNtpAsync();
        }

        private async void timeSetButton_Click(object sender, EventArgs e)
        {
            if (_beelinkTimeUiController != null)
                await _beelinkTimeUiController.SetManualTimeAsync(
                    manualDatePicker.Value, manualTimePicker.Value);
        }
        #endregion

        #region Обработчики UI
        private void beelinkClearLogButton_Click(object sender, EventArgs e) => _logManager?.ClearLog();

        private void beelinkLogTextBox_TextChanged(object sender, EventArgs e)
        {
            // Viewport position is owned by WinFormsLogSink.
            // Forcing ScrollToCaret here prevents reading earlier records.
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
        private void WebServerStartButton_Click(object? sender, EventArgs e)
        {
            if (_webServerManager == null)
            {
                MessageBox.Show("Менеджер веб-сервера не инициализирован",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _webServerManager.StartServer(ConfigService.Config.WebServer);
        }

        private void WebServerStopButton_Click(object? sender, EventArgs e) => _webServerManager?.StopServer();
        #endregion

        #region USR Transfer
        // Адаптеры для обработчиков, привязанных в MainForm.Designer.cs.
        // Вся логика представления ИР-0652 находится в USRTransferUiController.
        private void InitializeUsrTransferUi()
        {
            if (_usrTransferManager == null || _logManager == null) return;

            _usrTransferUiController = new USRTransferUiController(
                _usrTransferManager,
                new USRTransferControls(
                    cmbUsrRoutes, cmbInMode, lstSvcFiles, lstInFiles,
                    btnUsrTest, btnUsrSendSvc, btnUsrStartIn, btnUsrStopIn,
                    lblUsrStatus, lblUsrStep, lblUsrCountdown),
                _usrLog!.AppendLog);
        }

        private void cmbUsrRoutes_SelectedIndexChanged(object? sender, EventArgs e) =>
            _usrTransferUiController?.RouteChanged();

        private async void btnUsrTest_Click(object? sender, EventArgs e)
        {
            if (_usrTransferUiController != null)
                await _usrTransferUiController.TestConnectionAsync();
        }

        private async void btnUsrSendSvc_Click(object? sender, EventArgs e)
        {
            if (_usrTransferUiController != null)
                await _usrTransferUiController.SendSelectedSvcAsync();
        }

        private async void btnUsrStartIn_Click(object? sender, EventArgs e)
        {
            if (_usrTransferUiController != null)
                await _usrTransferUiController.StartInAsync();
        }

        private void btnUsrStopIn_Click(object? sender, EventArgs e) =>
            _usrTransferUiController?.StopIn();
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
            if (_hermesLog == null) return;

            var cfg = ConfigService.Config.HermesSSH;
            _hermesService = new HermesPassengerService(_hermesLog, cfg);
            _hermesManager = new HermesPassengerManager(_hermesService, _hermesLog);
            _hermesUiController = new HermesUiController(
                _hermesManager,
                new HermesControls(
                    hermesEnteredTextBox, hermesExitedTextBox,
                    btnHermesStart, btnHermesStop, btnHermesTest,
                    btnHermesUpdate, lblHermesStatus),
                _hermesLog.AppendLog);
        }

        // Designer event adapters. UI state/validation live in HermesUiController.
        private async void btnHermesStart_Click(object sender, EventArgs e)
        {
            if (_hermesUiController != null)
                await _hermesUiController.StartAsync();
        }

        private async void btnHermesStop_Click(object sender, EventArgs e)
        {
            if (_hermesUiController != null)
                await _hermesUiController.StopAsync();
        }

        private async void btnHermesTest_Click(object sender, EventArgs e)
        {
            if (_hermesUiController != null)
                await _hermesUiController.TestAsync();
        }

        private void btnHermesUpdate_Click(object sender, EventArgs e) =>
            _hermesUiController?.Update();

        #endregion

        #region События формы
        private void MainForm_Load(object sender, EventArgs e) => _logManager?.AppendLog("Приложение запущено");

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_shutdownCompleted)
                return;

            // Cancel the first close request, then close again after asynchronous
            // network workers have actually stopped. This keeps the UI responsive.
            e.Cancel = true;
            if (_shutdownStarted)
                return;

            _shutdownStarted = true;
            Enabled = false;

            try
            {
                await ShutdownServicesAsync();
            }
            catch (Exception ex)
            {
                try { _logManager?.AppendLog($"Ошибка завершения MKtest: {ex.Message}"); }
                catch { }
            }
            finally
            {
                // Every UI resource is released separately: one exception must
                // not leave the application permanently disabled.
                TryStop("Demo UI", () => _demoModeController?.Dispose());
                TryStop("ИР-0652 UI", () => _usrTransferUiController?.Dispose());
                TryStop("Гермес UI", () => _hermesUiController?.Dispose());
                TryStop("Время Beelink UI", () => _beelinkTimeUiController?.Dispose());
                TryStop("СЭКОП UI", () => _sekopUiController?.Dispose());
                TryStop("Панели", () => _collapsiblePanelController?.Dispose());
                TryStop("Левая колонка", () => _leftPanelLayout?.Dispose());
                TryStop("Правая колонка", () => _rightPanelLayout?.Dispose());

                TryStop("Журналы", () => _logManager?.AppendLog("Приложение закрывается"));
                try { _blockLogs?.Dispose(); } catch { }
                try { _logManager?.Dispose(); } catch { }

                _shutdownCompleted = true;
                if (!IsDisposed)
                    Close();
            }
        }

        private Task ShutdownServicesAsync()
        {
            var shutdown = new ApplicationShutdownCoordinator(
                message => _logManager?.AppendLog(message));

            shutdown.AddCancel("ИР-0652", () => _usrTransferManager?.StopInSequence());
            shutdown.AddCancel("JSON-RPC", () => _httpProtokolManager?.Stop());
            shutdown.AddCancel("HTTPpay", () => _httpPayManager?.Stop());
            shutdown.AddCancel("Demo", () => _demoScenarioService?.StopScenario());
            shutdown.AddCancel("СЭКОП", () => _sekopProtocolManager?.Stop());
            shutdown.AddCancel("МЧС", () => _emergencyService?.Dispose());

            shutdown.AddWait("ИР-0652", () =>
                _usrTransferManager?.StopInSequenceAsync() ?? Task.CompletedTask);
            shutdown.AddWait("JSON-RPC", () =>
                _httpProtokolManager?.StopAsync() ?? Task.CompletedTask);
            shutdown.AddWait("HTTPpay", () =>
                _httpPayManager?.StopAsync() ?? Task.CompletedTask);
            shutdown.AddWait("Demo", () =>
                _demoManager?.CleanupAsync() ??
                _demoScenarioService?.StopScenarioAsync() ?? Task.CompletedTask);
            shutdown.AddWait("Гермес", () =>
                _hermesManager?.StopAsync() ?? Task.CompletedTask);
            shutdown.AddWait("Время Beelink", () =>
                _timeManager?.WaitForIdleAsync() ?? Task.CompletedTask);
            shutdown.AddWait("СЭКОП", () =>
                _sekopProtocolManager?.StopAsync() ?? Task.CompletedTask);

            shutdown.AddDispose("SSH Beelink", () =>
                Task.Run(() => _sshService?.Dispose()));
            shutdown.AddDispose("Веб-сервер", () =>
                Task.Run(() => _webServerService?.Dispose()));
            shutdown.AddDispose("Гермес", () =>
                Task.Run(() => _hermesService?.Dispose()));
            shutdown.AddDispose("HTTPpay", () =>
                Task.Run(() => _httpPayService?.Dispose()));
            shutdown.AddDispose("СЭКОП TCP", () =>
                Task.Run(() => _sekopPacketSenderService?.Dispose()));

            return shutdown.RunAsync();
        }

        private void TryStop(string name, Action action)
        {
            try { action(); }
            catch (Exception ex)
            {
                try { _logManager?.AppendLog($"Остановка {name}: {ex.Message}"); }
                catch { }
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
            if (_httpProtokolService == null || _jsonLog == null) return;
            _httpProtokolManager = new JSONRPCprotokolManager(_httpProtokolService, _jsonLog);
            _jsonRpcUiController = new JsonRpcUiController(
                _httpProtokolManager, _httpProtokolService,
                httpProtokolTrCounterTextBox, httpProtokolIntervalTextBox,
                btnHttpProtokolStart, btnHttpProtokolStop, lblHttpStatus,
                _jsonLog.AppendLog);
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
                MessageBox.Show("Настройки Json-RPC Protokol сохранены!", "Успех",
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

            if (MessageBox.Show("Сбросить настройки Json-RPC Protokol к значениям по умолчанию?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _settingsManager.ResetHTTPProtokolSettings(_logManager);
            }
        }

        // WinForms Designer handlers delegate all protocol UI logic.
        private void btnHttpProtokolStart_Click(object sender, EventArgs e) =>
            _jsonRpcUiController?.Start();

        private void btnHttpProtokolStop_Click(object sender, EventArgs e) =>
            _jsonRpcUiController?.Stop();

        private async void btnHttpProtokolTest_Click(object sender, EventArgs e)
        {
            if (_jsonRpcUiController != null)
                await _jsonRpcUiController.TestAsync();
        }

        private void btnHttpProtokolUpdate_Click(object sender, EventArgs e) =>
            _jsonRpcUiController?.Update();
        #endregion

        #region HTTPpay Main

        private void InitializeHTTPpayMain()
        {
            if (_httpPayService == null || _httpPayLog == null) return;
            _httpPayManager = new HTTPpayManager(_httpPayService, _httpPayLog);
            _httpPayUiController = new HttpPayUiController(
                _httpPayManager,
                () => ConfigService.Config.HTTPpay.IntervalSeconds,
                httpPayTotalNumeric, ConfigService.Config.HTTPpay.TotalPayments,
                btnHttpPayStart, btnHttpPayStop, lblHttpPayStatus,
                _httpPayLog.AppendLog);
        }

        private void btnHttpPayStart_Click(object sender, EventArgs e) =>
            _httpPayUiController?.Start();

        private void btnHttpPayStop_Click(object sender, EventArgs e) =>
            _httpPayUiController?.Stop();

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
        // Designer продолжает вызывать обработчики кнопок в MainForm.
        // Состояние UI и отправка значений находятся в SekopUiController.
        private void InitializeSekopProtocolMain()
        {
            if (_sekopProtocolManager == null || _logManager == null) return;
            _sekopUiController = new SekopUiController(
                _sekopProtocolManager,
                new SekopControls(
                    sekopTransactionsNumeric, sekopPassengersNumeric,
                    sekopStartButton, sekopStopButton, sekopStatusLabel),
                _sekopLog!.AppendLog);
        }

        private async void sekopStartButton_Click(object sender, EventArgs e)
        {
            if (_sekopUiController != null)
                await _sekopUiController.StartAsync();
        }

        private void sekopStopButton_Click(object sender, EventArgs e) =>
            _sekopUiController?.Stop();

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

        #endregion

        #region МЧС
        // Existing buttons remain connected in MainForm.Designer.cs.
        private async void btnEmergency1_Click(object sender, EventArgs e)
        {
            if (_emergencyUiController != null)
                await _emergencyUiController.SendAsync(1);
        }

        private async void btnEmergency2_Click(object sender, EventArgs e)
        {
            if (_emergencyUiController != null)
                await _emergencyUiController.SendAsync(2);
        }

        private async void btnEmergency3_Click(object sender, EventArgs e)
        {
            if (_emergencyUiController != null)
                await _emergencyUiController.SendAsync(3);
        }

        private async void btnEmergency4_Click(object sender, EventArgs e)
        {
            if (_emergencyUiController != null)
                await _emergencyUiController.SendAsync(4);
        }
        #endregion

    }
}
