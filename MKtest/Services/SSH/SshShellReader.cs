using Renci.SshNet;
using System;
using System.Text;
using System.Threading;

namespace MKtest.Services.SSH
{
    /// <summary>
    /// Чтение интерактивного shell и удаление ANSI-последовательностей.
    /// Не создаёт сетевые соединения и не знает ничего о WinForms.
    /// </summary>
    public sealed class SshShellReader
    {
        private readonly ShellStream _stream;
        private readonly Action<string> _log;

        public SshShellReader(ShellStream stream, Action<string> log)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public string WaitForPrompt(string prompt, int timeoutMs)
        {
            var start = DateTime.UtcNow;
            var output = new StringBuilder();

            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                if (_stream.DataAvailable)
                {
                    var buffer = new byte[1024];
                    int count = _stream.Read(buffer, 0, buffer.Length);
                    if (count > 0)
                    {
                        string chunk = Encoding.UTF8.GetString(buffer, 0, count);
                        output.Append(chunk);
                        _log($"Получено: {chunk}");

                        // Also handles a prompt divided between two SSH packets.
                        if (output.ToString().Contains(prompt, StringComparison.Ordinal))
                            return output.ToString();
                    }
                }

                Thread.Sleep(100);
            }

            return output.ToString();
        }

        public string ReadAvailableOutput(int timeoutMs)
        {
            var start = DateTime.UtcNow;
            var output = new StringBuilder();

            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                if (!_stream.DataAvailable)
                    break;

                var buffer = new byte[1024];
                int count = _stream.Read(buffer, 0, buffer.Length);
                if (count > 0)
                    output.Append(Encoding.UTF8.GetString(buffer, 0, count));

                Thread.Sleep(50);
            }

            return output.ToString();
        }

        public static string CleanOutput(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var sb = new StringBuilder();
            bool inEscape = false;

            foreach (char c in input)
            {
                if (c == '\u001B')
                {
                    inEscape = true;
                    continue;
                }

                if (inEscape)
                {
                    if (c == 'm' || c == 'K' || c == 'J' || c == 'H' ||
                        c == 'A' || c == 'B' || c == 'C' || c == 'D')
                        inEscape = false;
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString()
                .Replace("[?2004h", "")
                .Replace("[?2004l", "")
                .Replace("\b", "")
                .Trim();
        }
    }
}
