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
        private USRTransferUiController? _usrTransferUiController;
        private SekopUiController? _sekopUiController;
        private BlockLogPanelRegistry? _blockLogs;
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
                beelinkConnectButton, beelinkDisconnectButton, beelinkTestButton, beelinkStatusLabel,
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
            if (_sekopProtocolService == null || _logManager == null)
                throw new InvalidOperationException("Сервисы протокола СЭКОП не инициализированы");
            _sekopProtocolManager = new SekopProtocolManager(_sekopProtocolService,_sekopLog!);

            if (_emergencyService == null)
                throw new InvalidOperationException("EmergencyService не инициализирован");
            _emergencyManager = new EmergencyManager(_emergencyService,ConfigService.Config.Emergency);
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

            webServerStartButton.Click += WebServerStartButton_Click;
            webServerStopButton.Click += WebServerStopButton_Click;

            _settingsManager?.LoadSettings();
            Resize += MainForm_Resize;
            _collapsiblePanelController.CollapseAll();

            if (_demoScenarioService != null)
            {
                _demoModeController = new DemoModeController(
                    rbAutoMode, rbManualMode, btnStartDemo, btnNextStep,
                    cmbDemoScenarios, _demoScenarioService,
                    message => _demoLog?.AppendLog(message));
            }
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            if (mainTabControl.SelectedTab == mainTabPage) UpdateLayout();
        }

        private void UpdateLayout() => leftFlowLayout?.PerformLayout();
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
            if (_logManager == null) return;

            var cfg = ConfigService.Config.HermesSSH;
            _hermesService = new HermesPassengerService(_hermesLog!, cfg);
            _hermesManager = new HermesPassengerManager(_hermesService, _hermesLog!);
            _hermesManager.StatusChanged += HermesManager_StatusChanged;

            hermesEnteredTextBox.Text = "0";
            hermesExitedTextBox.Text = "0";
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
                _hermesLog!.AppendLog($"Hermes start error: {ex.Message}");
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
                _hermesLog!.AppendLog($"Hermes stop error: {ex.Message}");
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
                _hermesLog!.AppendLog(ok
                    ? "Hermes SSH: подключение успешно"
                    : "Hermes SSH: подключение не удалось");
            }
            catch (Exception ex)
            {
                _hermesLog!.AppendLog($"Hermes test fatal: {ex.Message}");
                lblHermesStatus.Text = "Статус: Ошибка";
            }
        }

        private void btnHermesUpdate_Click(object sender, EventArgs e)
        {
            if (_hermesManager == null || _logManager == null) return;
            if (!TryReadHermesValues(out var entered, out var exited)) return;

            _hermesManager.UpdateValues(entered, exited);
            _hermesLog!.AppendLog($"Hermes: обновлены значения A={entered}, B={exited}");
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
                _usrTransferUiController?.Dispose();
                _sekopUiController?.Dispose();
                if (_sekopProtocolManager?.IsRunning == true)
                    _sekopProtocolManager.Stop();
                _sekopPacketSenderService?.Dispose();
                _sshService?.Dispose();
                _webServerService?.Dispose();
                _httpPayManager?.Stop();
                _httpPayService?.Dispose();
                _demoModeController?.Dispose();
                _collapsiblePanelController?.Dispose();
                _demoManager?.Cleanup();
                _logManager?.AppendLog("Приложение закрывается");
                if (_hermesManager != null)
                    _hermesManager.StopAsync().Wait(1500);
            }
            catch (Exception ex)
            {
                _logManager?.AppendLog($"Ошибка при закрытии: {ex.Message}");

            }
            finally
            {
                _blockLogs?.Dispose();
                _logManager?.Dispose();
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
            _httpProtokolManager = new JSONRPCprotokolManager(_httpProtokolService, _jsonLog!);
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
                lblHttpStatus.Text = "Статус: Запущено"; _jsonLog?.AppendLog("JSONRPC запущен");
            }
            catch (Exception ex)
            {
                _jsonLog?.AppendLog(ex.Message);
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
            _jsonLog?.AppendLog("JsonRPCprotokol остановлен");
        }
        private async void btnHttpProtokolTest_Click(object sender, EventArgs e)
        {
            if (_httpProtokolService == null)
                return;
            try
            {
                int trCounter = int.Parse(httpProtokolTrCounterTextBox.Text);
                await _httpProtokolService.SendAsync(trCounter, CancellationToken.None);
                _jsonLog?.AppendLog("JSONRPC тест отправлен");
            }
            catch (Exception ex)
            {
                _jsonLog?.AppendLog(ex.Message);
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

                _jsonLog?.AppendLog(
                    "JSONRPCprotokol параметры обновлены");
            }
            catch (Exception ex)
            {
                _jsonLog?.AppendLog(ex.Message);
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
                    _httpPayLog!);

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

            _httpPayLog!.AppendLog(
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
