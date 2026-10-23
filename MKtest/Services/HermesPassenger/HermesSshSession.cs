using Renci.SshNet;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.HermesPassenger
{
    /// <summary>
    /// Single SSH.NET client with exclusive command access. Two periodic
    /// workers must never issue RunCommand simultaneously on the same session.
    /// </summary>
    public sealed class HermesSshSession : IHermesSshSession
    {
        private readonly SshClient _client;
        private readonly SemaphoreSlim _commandGate = new(1, 1);
        private bool _disposed;

        public HermesSshSession(SshClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public bool IsConnected => !_disposed && _client.IsConnected;

        public async Task ConnectAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            // SSH.NET Connect() is synchronous. Await its completion before
            // allowing this client to be cleaned up by the owning service.
            await Task.Run(() => _client.Connect()).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            if (!_client.IsConnected)
                throw new InvalidOperationException("Не удалось установить SSH соединение");
        }

        public async Task<bool> ExecuteAsync(string command, CancellationToken token)
        {
            await _commandGate.WaitAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!IsConnected)
                    return false;

                // CreateCommand/Execute allow a finite command timeout, so Stop
                // cannot wait indefinitely for a stalled remote command.
                using var sshCommand = _client.CreateCommand(command);
                sshCommand.CommandTimeout = TimeSpan.FromSeconds(30);
                await Task.Run(() => sshCommand.Execute()).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                return true;
            }
            finally
            {
                _commandGate.Release();
            }
        }

        public async Task DisconnectAsync()
        {
            await _commandGate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_disposed) return;
                _disposed = true;
                try
                {
                    if (_client.IsConnected)
                        _client.Disconnect();
                }
                finally
                {
                    _client.Dispose();
                }
            }
            finally
            {
                _commandGate.Release();
            }
        }

        public void Dispose()
        {
            DisconnectAsync().GetAwaiter().GetResult();
            // The command gate is kept alive for idempotent Dispose/Disconnect.
        }
    }
}
