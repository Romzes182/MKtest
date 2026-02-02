using MKtest.Configs;
using MKtest.Managers;
using MKtest.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        #endregion

        #region Сервисы (объявляем как nullable)
        private SSHService? _sshService;
        private TimeCommandsService? _timeService;
        #endregion

        #region Поля для управления сворачиванием
        private bool _isBeelinkCollapsed = true;
        private int _beelinkCollapsiblePanelOriginalHeight;
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
                // Сохраняем исходную высоту панели
                _beelinkCollapsiblePanelOriginalHeight = beelinkCollapsiblePanel.Height;

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
        #endregion

        #region Инициализация сервисов
        private void InitializeServices()
        {
            _sshService = new SSHService();
            _timeService = new TimeCommandsService(_sshService);
            _webServerService = new WebServerService(_logManager);
        }
        #endregion

        #region Инициализация менеджеров
        private void InitializeManagers()
        {
            // 1. Менеджер логов
            _logManager = new LogManager(beelinkLogTextBox);

            // 2. Менеджер состояния SSH UI
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

            // 3. Менеджер состояния веб-сервера
            _webServerStateManager = new WebServerStateManager(
                webServerStartButton,
                webServerStopButton,
                webServerStatusLabel
            );

            // 4. Менеджер настроек (обновлен для веб-сервера)
            _settingsManager = new SettingsManager(
                ipTextBox,
                portNumeric,
                userTextBox,
                passwordUserTextBox,
                passwordRootTextBox,
                webServerIpTextBox,      // Добавлено
                webServerPortNumeric     // Добавлено
            );

            // 5. Менеджер SSH подключения
            if (_sshService == null || _logManager == null || _stateManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");

            _sshManager = new SSHConnectionManager(_sshService, _logManager, _stateManager);

            // 6. Менеджер веб-сервера
            if (_webServerService == null || _logManager == null || _webServerStateManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");

            _webServerManager = new WebServerManager(_webServerService, _logManager, _webServerStateManager);

            // 7. Менеджер команд времени
            if (_timeService == null || _logManager == null || _sshManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");

            _timeManager = new TimeCommandsManager(_timeService, _logManager, () =>
                _sshManager.IsConnected()
            );
        }
        #endregion

        #region Настройка UI
        private void SetupUI()
        {
            // Настройка контролов времени
            manualDatePicker.Format = DateTimePickerFormat.Short;
            manualTimePicker.Format = DateTimePickerFormat.Time;
            manualTimePicker.ShowUpDown = true;

            // Настройка сворачиваемой панели SSH
            beelinkHeaderPanel.Click += BeelinkHeaderPanel_Click;
            beelinkHeaderLabel.Click += BeelinkHeaderPanel_Click;
            UpdateCollapsiblePanelState();

            // Настройка сворачиваемой панели веб-сервера
            webServerHeaderPanel.Click += WebServerHeaderPanel_Click;
            webServerHeaderLabel.Click += WebServerHeaderPanel_Click;
            _webServerCollapsiblePanelOriginalHeight = webServerCollapsiblePanel.Height;
            UpdateWebServerCollapsiblePanelState();

            // Обработчики кнопок веб-сервера
            webServerStartButton.Click += WebServerStartButton_Click;
            webServerStopButton.Click += WebServerStopButton_Click;

            // Загрузка настроек
            _settingsManager?.LoadSettings();
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
        #endregion

        #region Обработчики сворачиваемой панели
        private void BeelinkHeaderPanel_Click(object sender, EventArgs e)
        {
            _isBeelinkCollapsed = !_isBeelinkCollapsed;
            UpdateCollapsiblePanelState();
        }

        private void UpdateCollapsiblePanelState()
        {
            if (_isBeelinkCollapsed)
            {
                // Сворачиваем
                beelinkContentPanel.Visible = false;
                beelinkCollapsiblePanel.Height = beelinkHeaderPanel.Height + 2; // +2 для границы
                beelinkHeaderLabel.Text = "SSH Beelink ▶"; // Стрелка вправо для свернутого состояния
            }
            else
            {
                // Разворачиваем
                beelinkContentPanel.Visible = true;
                beelinkCollapsiblePanel.Height = _beelinkCollapsiblePanelOriginalHeight;
                beelinkHeaderLabel.Text = "SSH Beelink ▼"; // Стрелка вниз для развернутого состояния
            }
        }
        #endregion

        #region Дополнительные обработчики
        private void beelinkLogTextBox_TextChanged(object sender, EventArgs e)
        {
            // Автопрокрутка текстового поля лога
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

            // Используем SaveSshSettings вместо SaveSettings
            if (_settingsManager.SaveSshSettings(_logManager))
            {
                MessageBox.Show("Настройки SSH Beelink сохранены!",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Ошибка сохранения настроек SSH",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                // Используем ResetSshSettings вместо ResetSettings
                if (_settingsManager.ResetSshSettings(_logManager))
                {
                    MessageBox.Show("Настройки SSH Beelink сброшены к значениям по умолчанию!",
                        "Сброс", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка сброса настроек SSH",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            else
            {
                MessageBox.Show("Ошибка сохранения настроек веб-сервера",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (_settingsManager.ResetWebServerSettings(_logManager))
                {
                    MessageBox.Show("Настройки веб-сервера сброшены к значениям по умолчанию!",
                        "Сброс", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка сброса настроек веб-сервера",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        private void WebServerHeaderPanel_Click(object sender, EventArgs e)
        {
            _isWebServerCollapsed = !_isWebServerCollapsed;
            UpdateWebServerCollapsiblePanelState();
        }

        private void UpdateWebServerCollapsiblePanelState()
        {
            if (_isWebServerCollapsed)
            {
                // Сворачиваем
                webServerContentPanel.Visible = false;
                webServerCollapsiblePanel.Height = webServerHeaderPanel.Height + 2;
                webServerHeaderLabel.Text = "Веб-сервер ▶";
            }
            else
            {
                // Разворачиваем
                webServerContentPanel.Visible = true;
                webServerCollapsiblePanel.Height = _webServerCollapsiblePanelOriginalHeight;
                webServerHeaderLabel.Text = "Веб-сервер ▼";
            }
        }
        #endregion

        #region Веб-сервер
        private WebServerService? _webServerService;
        private WebServerManager? _webServerManager;
        private WebServerStateManager? _webServerStateManager;
        private bool _isWebServerCollapsed = true;
        private int _webServerCollapsiblePanelOriginalHeight;
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
                _logManager?.AppendLog("Приложение закрывается");
            }
            catch (Exception ex)
            {
                _logManager?.AppendLog($"Ошибка при закрытии: {ex.Message}");
            }
        }

        private void webServerContentPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void flowLayoutPanel_Paint(object sender, PaintEventArgs e)
        {

        }
        #endregion

        // Удалите эти два пустых метода:
        // private void webServerSaveButton_Click_1(object sender, EventArgs e) { }
        // private void webServerResetButton_Click_1(object sender, EventArgs e) { }
    }
}