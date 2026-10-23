using Renci.SshNet;
using System;
using System.Threading;

namespace MKtest.Services.SSH
{
    /// <summary>
    /// Одна интерактивная shell-сессия. Управляет shell, но не SSH-клиентом.
    /// </summary>
    public sealed class SshShellSession : IDisposable
    {
        private readonly ShellStream _shell;
        private readonly SshShellReader _reader;
        private readonly SshRootAuthenticator _authenticator;
        private readonly Action<string> _log;
        private bool _disposed;

        public SshShellSession(SshClient client, Action<string> log)
        {
            ArgumentNullException.ThrowIfNull(client);
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _shell = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
            _reader = new SshShellReader(_shell, log);
            _authenticator = new SshRootAuthenticator(_shell, _reader);
        }

        public bool TryLoginAsRoot(string password, out string whoamiResult) =>
            _authenticator.TryAuthenticate(password, out whoamiResult);

        public string ExecuteCommand(string command)
        {
            if (_disposed)
                return "ERROR: Нет подключения или не выполнен вход под root";

            try
            {
                _shell.WriteLine(command);
                _log($"Выполняется команда: {command}");
                Thread.Sleep(1000);

                string output = _reader.ReadAvailableOutput(3000);
                return SshShellReader.CleanOutput(output);
            }
            catch (Exception ex)
            {
                _log($"Ошибка выполнения команды: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }

        public void Logout(bool wasRoot)
        {
            if (_disposed) return;

            try
            {
                if (wasRoot)
                {
                    _shell.WriteLine("exit");
                    Thread.Sleep(500);
                }

                _shell.WriteLine("exit");
            }
            catch (Exception)
            {
                // Remote side might already have closed the shell.
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _shell.Close();
            _shell.Dispose();
        }
    }
}
