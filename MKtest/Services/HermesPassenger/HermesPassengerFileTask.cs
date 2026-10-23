using MKtest.Configs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.HermesPassenger
{
    /// <summary>
    /// Creates exactly the original A;B;61440 payload and the echo command.
    /// It reads an atomic pair of current values on each send.
    /// </summary>
    public sealed class HermesPassengerFileTask
    {
        private readonly IHermesSshSession _session;
        private readonly HermesSSHConfig _config;
        private readonly Func<HermesPassengerValues> _readValues;
        private readonly Action<string> _log;

        public HermesPassengerFileTask(
            IHermesSshSession session,
            HermesSSHConfig config,
            Func<HermesPassengerValues> readValues,
            Action<string> log)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _readValues = readValues ?? throw new ArgumentNullException(nameof(readValues));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            var current = _readValues();
            var payload = $"{current.Entered};{current.Exited};61440";
            var command = $"echo '{payload}' > {_config.RemoteFilePath}";
            if (await _session.ExecuteAsync(command, token).ConfigureAwait(false))
                _log($"Гермес: файл обновлён {payload}");
        }
    }
}
