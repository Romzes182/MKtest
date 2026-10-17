using Renci.SshNet;
using System;
using System.Threading;

namespace MKtest.Services.SSH
{
    /// <summary>
    /// Интерактивная авторизация через su (отдельно от SSH password authentication).
    /// Порядок команд и задержки совпадают с прежним SSHService.
    /// </summary>
    public sealed class SshRootAuthenticator
    {
        private readonly ShellStream _shell;
        private readonly SshShellReader _reader;

        public SshRootAuthenticator(ShellStream shell, SshShellReader reader)
        {
            _shell = shell ?? throw new ArgumentNullException(nameof(shell));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public bool TryAuthenticate(string rootPassword, out string whoamiResult)
        {
            _reader.WaitForPrompt("user@", 2000);

            _shell.WriteLine("su");
            Thread.Sleep(500);
            _reader.WaitForPrompt("Пароль:", 2000);

            _shell.WriteLine(rootPassword);
            Thread.Sleep(1000);
            _shell.WriteLine("whoami");
            Thread.Sleep(1000);

            whoamiResult = _reader.ReadAvailableOutput(2000);
            return whoamiResult.Contains("root", StringComparison.Ordinal);
        }
    }
}
