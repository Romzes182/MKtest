using MKtest.Configs;
using MKtest.Services.SSH;
using Renci.SshNet;
using System;
using System.Threading.Tasks;

namespace MKtest.Services
{
    /// <summary>
    /// Управляет жизненным циклом SSH-подключения Beelink.
    /// Авторизация root, чтение shell и выполнение команд вынесены в отдельные классы.
    /// </summary>
    public sealed class SSHService : ISshCommandExecutor, IDisposable
    {
        private readonly ISshClientFactory _clientFactory;
        private SshClient? _sshClient;
        private SshShellSession? _shellSession;
        private bool _isConnected;
        private bool _isRootLoggedIn;

        public event Action<string>? OnLogMessage;
        public event Action<string>? OnStatusChanged;

        public SSHService(ISshClientFactory? clientFactory = null)
        {
            _clientFactory = clientFactory ?? new SshClientFactory();
        }

        public Task<bool> ConnectAsync(SSHConfig config) =>
            Task.Run(() => Connect(config));

        public bool Connect(SSHConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            try
            {
                if (_sshClient != null)
                    ReleaseConnection();

                OnLogMessage?.Invoke($"Попытка подключения к {config.IP}:{config.Port} как {config.User}");
                OnStatusChanged?.Invoke($"Подключение к {config.IP}...");

                var client = _clientFactory.Create(config);
                _sshClient = client;
                client.Connect();

                if (!client.IsConnected)
                {
                    OnLogMessage?.Invoke("Ошибка: SSH соединение не установлено");
                    OnStatusChanged?.Invoke("Ошибка подключения");
                    ReleaseConnection();
                    return false;
                }

                OnLogMessage?.Invoke("SSH подключение установлено успешно");

                var shellSession = new SshShellSession(client, Log);
                _shellSession = shellSession;

                if (!shellSession.TryLoginAsRoot(config.PasswordRoot, out var whoamiResult))
                {
                    OnLogMessage?.Invoke($"Не удалось войти под root. Ответ whoami: {whoamiResult}");
                    OnStatusChanged?.Invoke("Ошибка авторизации root");
                    ReleaseConnection();
                    return false;
                }

                _isConnected = true;
                _isRootLoggedIn = true;
                OnLogMessage?.Invoke("Успешный вход под root");
                OnStatusChanged?.Invoke("Подключено (root)");
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Ошибка SSH подключения: {ex.Message}");
                OnStatusChanged?.Invoke($"Ошибка: {ex.Message}");
                ReleaseConnection();
                return false;
            }
        }

        public string ExecuteCommand(string command)
        {
            if (!IsConnected() || _shellSession == null)
                return "ERROR: Нет подключения или не выполнен вход под root";

            return _shellSession.ExecuteCommand(command);
        }

        public string ExecuteDirectCommand(string command)
        {
            if (!IsConnected() || _sshClient == null)
                return "ERROR: Нет подключения";

            return SshDirectCommandExecutor.Execute(_sshClient, command, Log);
        }

        public void Disconnect()
        {
            try
            {
                ReleaseConnection();
                OnLogMessage?.Invoke("SSH соединение закрыто");
                OnStatusChanged?.Invoke("Отключено");
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Ошибка при отключении: {ex.Message}");
            }
        }

        public bool IsConnected() =>
            _isConnected &&
            _isRootLoggedIn &&
            _sshClient?.IsConnected == true;

        private void Log(string message) => OnLogMessage?.Invoke(message);

        private void ReleaseConnection()
        {
            var shellSession = _shellSession;
            var client = _sshClient;
            bool wasRoot = _isRootLoggedIn;

            _isConnected = false;
            _isRootLoggedIn = false;
            _shellSession = null;
            _sshClient = null;

            if (shellSession != null)
            {
                try
                {
                    shellSession.Logout(wasRoot);
                }
                catch (Exception ex)
                {
                    Log($"Ошибка выхода из shell: {ex.Message}");
                }
                finally
                {
                    try { shellSession.Dispose(); }
                    catch (Exception ex) { Log($"Ошибка закрытия shell: {ex.Message}"); }
                }
            }

            if (client != null)
            {
                try
                {
                    if (client.IsConnected) client.Disconnect();
                }
                finally
                {
                    client.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Disconnect();
            GC.SuppressFinalize(this);
        }
    }
}
