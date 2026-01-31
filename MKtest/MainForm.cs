using MKtest.Configs;
using MKtest.Services;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKtest
{
    public partial class MainForm : Form
    {
        #region Поля
        private SSHService? _sshService;
        private TimeCommandsService? _timeCommandsService;
        private bool _servicesInitialized = false;
        #endregion

        #region Конструктор
        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            SetupUI();
        }
        #endregion

        #region Инициализация
        private void InitializeServices()
        {
            if (_servicesInitialized) return;

            _sshService = new SSHService();
            _timeCommandsService = new TimeCommandsService(_sshService);

            // Подписываемся на события сервисов
            _sshService.OnLogMessage += OnSSHLogMessage;
            _sshService.OnStatusChanged += OnSSHStatusChanged;
            _timeCommandsService.OnCommandExecuted += OnTimeCommandExecuted;

            _servicesInitialized = true;
        }

        private void SetupUI()
        {
            // Загружаем настройки
            LoadSettings();

            // Настраиваем лог
            beelinkLogTextBox.ReadOnly = true;
            beelinkLogTextBox.ScrollBars = ScrollBars.Vertical;
            beelinkLogTextBox.Font = new Font("Consolas", 9);

            // Настраиваем контролы времени
            manualDatePicker.Format = DateTimePickerFormat.Short;
            manualTimePicker.Format = DateTimePickerFormat.Time;
            manualTimePicker.ShowUpDown = true;

            // Обновляем состояние кнопок
            UpdateConnectionButtons();
        }
        #endregion

        #region Обработчики событий
        private void OnSSHLogMessage(string message)
        {
            AppendLog(message);
        }

        private void OnSSHStatusChanged(string status)
        {
            if (beelinkStatusLabel.InvokeRequired)
            {
                beelinkStatusLabel.Invoke(new Action(() =>
                {
                    beelinkStatusLabel.Text = $"Статус: {status}";
                }));
            }
            else
            {
                beelinkStatusLabel.Text = $"Статус: {status}";
            }

            // Обновляем состояние кнопок в UI потоке
            UpdateConnectionButtons();
        }

        private void OnTimeCommandExecuted(string message)
        {
            AppendLog(message);
        }
        #endregion

        #region SSH методы
        private async void beelinkConnectButton_Click(object sender, EventArgs e)
        {
            if (_sshService == null) return;

            try
            {
                var config = ConfigService.Config.SSHBeelink;

                beelinkConnectButton.Enabled = false;
                beelinkTestButton.Enabled = false;

                // Подключаемся асинхронно
                bool connected = await _sshService.ConnectAsync(config);

                if (connected)
                {
                    MessageBox.Show("SSH подключение установлено!",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось установить SSH подключение",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendLog($"Ошибка подключения: {ex.Message}");
            }
            finally
            {
                UpdateConnectionButtons();
            }
        }

        private void beelinkDisconnectButton_Click(object sender, EventArgs e)
        {
            _sshService?.Disconnect();
        }

        private void beelinkTestButton_Click(object sender, EventArgs e)
        {
            if (_sshService != null && _sshService.IsConnected())
            {
                // Тестируем команду
                var result = _sshService.ExecuteDirectCommand("echo 'SSH Test OK'");
                AppendLog($"Тест подключения: {result}");
            }
            else
            {
                // Пытаемся подключиться
                beelinkConnectButton_Click(sender, e);
            }
        }
        #endregion

        #region Команды времени (только 4 кнопки осталось)
        private void timeCheckButton_Click(object sender, EventArgs e)
        {
            if (!CheckConnection()) return;

            var result = _timeCommandsService?.CheckTimeStatus() ?? "Ошибка: сервис не инициализирован";
            AppendLog($"Статус времени:\n{result}");
        }

        private void timeEnableNTPButton_Click(object sender, EventArgs e)
        {
            if (!CheckConnection()) return;

            var result = _timeCommandsService?.EnableNTP() ?? "Ошибка: сервис не инициализирован";
            AppendLog(result);
        }

        private void timeDisableNTPButton_Click(object sender, EventArgs e)
        {
            if (!CheckConnection()) return;

            var result = _timeCommandsService?.DisableNTP() ?? "Ошибка: сервис не инициализирован";
            AppendLog(result);
        }

        private void timeSetButton_Click(object sender, EventArgs e)
        {
            if (!CheckConnection()) return;

            var dateTime = $"{manualDatePicker.Value:yyyy-MM-dd} {manualTimePicker.Value:HH:mm:ss}";
            var result = _timeCommandsService?.SetManualDateTime(dateTime) ?? "Ошибка: сервис не инициализирован";
            AppendLog(result);
        }
        #endregion

        #region Вспомогательные методы
        private bool CheckConnection()
        {
            if (_sshService == null || !_sshService.IsConnected())
            {
                MessageBox.Show("Сначала установите SSH подключение",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void AppendLog(string message)
        {
            if (beelinkLogTextBox.InvokeRequired)
            {
                beelinkLogTextBox.Invoke(new Action(() =>
                {
                    beelinkLogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
                    beelinkLogTextBox.ScrollToCaret();
                }));
            }
            else
            {
                beelinkLogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
                beelinkLogTextBox.ScrollToCaret();
            }
        }

        private void UpdateConnectionButtons()
        {
            bool isConnected = _sshService?.IsConnected() ?? false;

            // Обновляем состояние кнопок в UI потоке
            if (beelinkConnectButton.InvokeRequired)
            {
                beelinkConnectButton.Invoke(new Action(() =>
                {
                    UpdateButtonsState(isConnected);
                }));
            }
            else
            {
                UpdateButtonsState(isConnected);
            }
        }

        private void UpdateButtonsState(bool isConnected)
        {
            beelinkConnectButton.Enabled = !isConnected;
            beelinkDisconnectButton.Enabled = isConnected;
            beelinkTestButton.Enabled = true;

            // Активируем кнопки команд времени только при подключении
            timeCheckButton.Enabled = isConnected;
            timeEnableNTPButton.Enabled = isConnected;
            timeDisableNTPButton.Enabled = isConnected;
            timeSetButton.Enabled = isConnected;
            manualDatePicker.Enabled = isConnected;
            manualTimePicker.Enabled = isConnected;
            timeGroupBox.Enabled = isConnected;
        }

        private void beelinkClearLogButton_Click(object sender, EventArgs e)
        {
            beelinkLogTextBox.Clear();
        }
        #endregion

        #region Настройки
        private void LoadSettings()
        {
            var config = ConfigService.Config.SSHBeelink;

            ipTextBox.Text = config.IP;
            portNumeric.Value = config.Port;
            userTextBox.Text = config.User;
            passwordUserTextBox.Text = config.PasswordUser;
            passwordRootTextBox.Text = config.PasswordRoot;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                var config = ConfigService.Config.SSHBeelink;

                config.IP = ipTextBox.Text;
                config.Port = (int)portNumeric.Value;
                config.User = userTextBox.Text;
                config.PasswordUser = passwordUserTextBox.Text;
                config.PasswordRoot = passwordRootTextBox.Text;

                ConfigService.Save();
                MessageBox.Show("Настройки SSH Beelink сохранены!",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                AppendLog("Настройки SSH сохранены");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendLog($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Сбросить настройки SSH Beelink к значениям по умолчанию?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    ConfigService.Config.SSHBeelink = new SSHConfig();
                    ConfigService.Save();
                    LoadSettings();

                    MessageBox.Show("Настройки сброшены к значениям по умолчанию!",
                        "Сброс", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    AppendLog("Настройки SSH сброшены к значениям по умолчанию");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сброса: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    AppendLog($"Ошибка сброса настроек: {ex.Message}");
                }
            }
        }
        #endregion

        #region События формы
        private void MainForm_Load(object sender, EventArgs e)
        {
            AppendLog("Приложение запущено");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _sshService?.Dispose();
                AppendLog("Приложение закрывается");
            }
            catch (Exception ex)
            {
                AppendLog($"Ошибка при закрытии: {ex.Message}");
            }
        }
        #endregion

        #region Пустые обработчики
        private void timeGroupBox_Enter(object sender, EventArgs e)
        {
            // Пустой обработчик, созданный конструктором
        }
        #endregion
    }
}