using MKtest.Configs;
using MKtest.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKtest.Managers
{
    public class SSHConnectionManager
    {
        private readonly SSHService _sshService;
        private readonly LogManager _logManager;
        private readonly ConnectionStateManager _stateManager;

        public SSHConnectionManager(SSHService sshService, LogManager logManager, ConnectionStateManager stateManager)
        {
            _sshService = sshService;
            _logManager = logManager;
            _stateManager = stateManager;

            // Подписываемся на события
            _sshService.OnLogMessage += OnSSHLogMessage;
            _sshService.OnStatusChanged += OnSSHStatusChanged;
        }

        private void OnSSHLogMessage(string message) => _logManager.AppendLog(message);

        private void OnSSHStatusChanged(string status)
        {
            _stateManager.UpdateStatusLabel($"Статус: {status}");
            _stateManager.UpdateConnectionButtons(_sshService.IsConnected());
        }

        public async Task<bool> ConnectAsync(SSHConfig config)
        {
            try
            {
                _stateManager.SetButtonsEnabled(false, false, true);
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

                return connected;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logManager.AppendLog($"Ошибка подключения: {ex.Message}");
                return false;
            }
            finally
            {
                _stateManager.UpdateConnectionButtons(_sshService.IsConnected());
            }
        }

        public void Disconnect()
        {
            _sshService.Disconnect();
        }

        public void TestConnection()
        {
            if (_sshService.IsConnected())
            {
                var result = _sshService.ExecuteDirectCommand("echo 'SSH Test OK'");
                _logManager.AppendLog($"Тест подключения: {result}");
            }
        }

        public bool IsConnected() => _sshService.IsConnected();
    }
}