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
        }
        #endregion

        #region Инициализация менеджеров
        private void InitializeManagers()
        {
            // 1. Менеджер логов
            _logManager = new LogManager(beelinkLogTextBox);

            // 2. Менеджер состояния UI
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

            // 3. Менеджер настроек
            _settingsManager = new SettingsManager(
                ipTextBox,
                portNumeric,
                userTextBox,
                passwordUserTextBox,
                passwordRootTextBox
            );

            // 4. Менеджер SSH подключения (зависит от сервисов и других менеджеров)
            if (_sshService == null || _logManager == null || _stateManager == null)
                throw new InvalidOperationException("Зависимые сервисы не инициализированы");

            _sshManager = new SSHConnectionManager(_sshService, _logManager, _stateManager);

            // 5. Менеджер команд времени
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

        #region Обработчики настроек
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (_settingsManager == null || _logManager == null)
            {
                MessageBox.Show("Менеджеры не инициализированы",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_settingsManager.SaveSettings(_logManager))
            {
                MessageBox.Show("Настройки SSH Beelink сохранены!",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Ошибка сохранения настроек",
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
                if (_settingsManager.ResetSettings(_logManager))
                {
                    MessageBox.Show("Настройки сброшены к значениям по умолчанию!",
                        "Сброс", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка сброса настроек",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
                _logManager?.AppendLog("Приложение закрывается");
            }
            catch (Exception ex)
            {
                _logManager?.AppendLog($"Ошибка при закрытии: {ex.Message}");
            }
        }
        #endregion

        #region Пустые обработчики (созданы конструктором)
        private void timeGroupBox_Enter(object sender, EventArgs e)
        {
            // Пустой обработчик
        }
        #endregion
    }
}