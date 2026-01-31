using Renci.SshNet;
using MKtest.Configs;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services
{
    public class SSHService : IDisposable
    {
        private SshClient? _sshClient;
        private ShellStream? _shellStream;
        private bool _isConnected = false;
        private bool _isRootLoggedIn = false;
        private readonly object _lockObject = new();

        public event Action<string>? OnLogMessage;
        public event Action<string>? OnStatusChanged;

        public async Task<bool> ConnectAsync(SSHConfig config)
        {
            return await Task.Run(() => Connect(config));
        }

        public bool Connect(SSHConfig config)
        {
            try
            {
                OnLogMessage?.Invoke($"Попытка подключения к {config.IP}:{config.Port} как {config.User}");
                OnStatusChanged?.Invoke($"Подключение к {config.IP}...");

                // Создаем подключение
                var connectionInfo = new ConnectionInfo(
                    config.IP,
                    config.Port,
                    config.User,
                    new PasswordAuthenticationMethod(config.User, config.PasswordUser)
                );

                connectionInfo.Encoding = Encoding.UTF8;
                connectionInfo.Timeout = TimeSpan.FromSeconds(30);

                _sshClient = new SshClient(connectionInfo);
                _sshClient.Connect();

                if (!_sshClient.IsConnected)
                {
                    OnLogMessage?.Invoke("Ошибка: SSH соединение не установлено");
                    OnStatusChanged?.Invoke("Ошибка подключения");
                    return false;
                }

                OnLogMessage?.Invoke("SSH подключение установлено успешно");

                // Создаем shell stream
                _shellStream = _sshClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

                // Ждем приглашение shell
                WaitForPrompt("user@", 2000);

                // Отправляем su
                _shellStream.WriteLine("su");
                Thread.Sleep(500);

                // Ждем запрос пароля
                WaitForPrompt("Пароль:", 2000);

                // Отправляем пароль root
                _shellStream.WriteLine(config.PasswordRoot);
                Thread.Sleep(1000);

                // Проверяем, что вошли в root
                _shellStream.WriteLine("whoami");
                Thread.Sleep(1000);

                // Читаем ответ
                string whoamiResult = ReadAvailableOutput(2000);

                if (whoamiResult.Contains("root"))
                {
                    _isConnected = true;
                    _isRootLoggedIn = true;
                    OnLogMessage?.Invoke("Успешный вход под root");
                    OnStatusChanged?.Invoke("Подключено (root)");
                    return true;
                }
                else
                {
                    OnLogMessage?.Invoke($"Не удалось войти под root. Ответ whoami: {whoamiResult}");
                    OnStatusChanged?.Invoke("Ошибка авторизации root");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Ошибка SSH подключения: {ex.Message}");
                OnStatusChanged?.Invoke($"Ошибка: {ex.Message}");
                Disconnect();
                return false;
            }
        }

        private string WaitForPrompt(string prompt, int timeoutMs)
        {
            DateTime start = DateTime.Now;
            StringBuilder output = new StringBuilder();

            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                if (_shellStream == null) break;

                if (_shellStream.DataAvailable)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = _shellStream.Read(buffer, 0, buffer.Length);
                    string text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    output.Append(text);

                    OnLogMessage?.Invoke($"Получено: {text}");

                    if (text.Contains(prompt))
                    {
                        return output.ToString();
                    }
                }
                Thread.Sleep(100);
            }

            return output.ToString();
        }

        private string ReadAvailableOutput(int timeoutMs)
        {
            DateTime start = DateTime.Now;
            StringBuilder output = new StringBuilder();

            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                if (_shellStream == null) break;

                if (_shellStream.DataAvailable)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = _shellStream.Read(buffer, 0, buffer.Length);
                    string text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    output.Append(text);
                }
                else
                {
                    break;
                }
                Thread.Sleep(50);
            }

            return output.ToString();
        }

        public string ExecuteCommand(string command)
        {
            if (!_isConnected || _shellStream == null || !_isRootLoggedIn)
            {
                return "ERROR: Нет подключения или не выполнен вход под root";
            }

            try
            {
                // Отправляем команду
                _shellStream.WriteLine(command);
                OnLogMessage?.Invoke($"Выполняется команда: {command}");

                // Ждем ответ
                Thread.Sleep(1000);

                // Читаем доступный вывод
                string result = ReadAvailableOutput(3000);

                return CleanOutput(result);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Ошибка выполнения команды: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }

        public string ExecuteDirectCommand(string command)
        {
            if (!_isConnected || _sshClient == null || !_isRootLoggedIn)
            {
                return "ERROR: Нет подключения";
            }

            try
            {
                using var cmd = _sshClient.CreateCommand(command);
                cmd.CommandTimeout = TimeSpan.FromSeconds(10);

                OnLogMessage?.Invoke($"Выполнение команды: {command}");
                string result = cmd.Execute();
                string error = cmd.Error;

                if (!string.IsNullOrEmpty(error))
                {
                    OnLogMessage?.Invoke($"Ошибка команды: {error}");
                    return $"{result}\nERROR: {error}";
                }

                return result.Trim();
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Ошибка выполнения команды: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }

        private static string CleanOutput(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Удаляем escape-последовательности ANSI
            var sb = new StringBuilder();
            bool inEscape = false;

            foreach (char c in input)
            {
                if (c == '\u001B') // ESC character
                {
                    inEscape = true;
                    continue;
                }

                if (inEscape)
                {
                    if (c == 'm' || c == 'K' || c == 'J' || c == 'H' || c == 'A' || c == 'B' || c == 'C' || c == 'D')
                    {
                        inEscape = false;
                    }
                    continue;
                }

                sb.Append(c);
            }

            string cleaned = sb.ToString()
                .Replace("[?2004h", "")
                .Replace("[?2004l", "")
                .Replace("\b", "")  // Удаляем backspace
                .Trim();

            return cleaned;
        }

        public void Disconnect()
        {
            try
            {
                _isConnected = false;
                _isRootLoggedIn = false;

                if (_shellStream != null)
                {
                    try
                    {
                        // Если подключены, выходим из root и затем из сессии
                        if (_isRootLoggedIn)
                        {
                            _shellStream.WriteLine("exit");
                            Thread.Sleep(500);
                        }
                        _shellStream.WriteLine("exit");
                    }
                    catch { }

                    _shellStream?.Close();
                    _shellStream?.Dispose();
                    _shellStream = null;
                }

                _sshClient?.Disconnect();
                _sshClient?.Dispose();
                _sshClient = null;

                OnLogMessage?.Invoke("SSH соединение закрыто");
                OnStatusChanged?.Invoke("Отключено");
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Ошибка при отключении: {ex.Message}");
            }
        }

        public bool IsConnected()
        {
            return _isConnected &&
                   _sshClient != null &&
                   _sshClient.IsConnected &&
                   _isRootLoggedIn;
        }

        public void Dispose()
        {
            Disconnect();
            GC.SuppressFinalize(this);
        }
    }
}