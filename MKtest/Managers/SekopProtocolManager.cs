using MKtest.Services.SekopProtocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    public readonly record struct SekopValues(int Transactions, int Passengers);

    /// <summary>
    /// Управляет состоянием сеанса и исключает параллельный TCP-обмен.
    /// Пакеты строятся и отправляются независимыми сервисами.
    /// </summary>
    public sealed class SekopProtocolManager : ISekopProtocolOperations
    {
        private readonly SekopProtocolService _service;
        private readonly LogManager _logManager;
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private readonly object _stateLock = new();

        private Func<SekopValues>? _getValues;
        private bool _running;
        private int _sessionId;

        public bool IsRunning
        {
            get { lock (_stateLock) return _running; }
        }

        public SekopProtocolManager(SekopProtocolService service, LogManager logManager)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
        }

        public async Task StartAsync(Func<SekopValues> getValues)
        {
            ArgumentNullException.ThrowIfNull(getValues);

            int sessionId;
            lock (_stateLock)
            {
                if (_running)
                    return;

                _getValues = getValues;
                _running = true;
                sessionId = ++_sessionId;
            }

            try
            {
                await SendCurrentAsync();
            }
            catch
            {
                bool shouldDisconnect = false;
                lock (_stateLock)
                {
                    if (_sessionId == sessionId)
                    {
                        _running = false;
                        _getValues = null;
                        ++_sessionId;
                        shouldDisconnect = true;
                    }
                }

                if (shouldDisconnect)
                    _service.Disconnect();

                throw;
            }
        }

        public async Task SendCurrentAsync()
        {
            int sessionId;
            lock (_stateLock)
            {
                if (!_running)
                    return;

                sessionId = _sessionId;
            }

            // NumericUpDown.ValueChanged can fire again while the previous
            // TCP connection is still sending. Each packet must be serialized.
            await _sendLock.WaitAsync();
            try
            {
                Func<SekopValues>? provider;
                lock (_stateLock)
                {
                    if (!_running || _sessionId != sessionId)
                        return;

                    provider = _getValues;
                }

                if (provider == null)
                    throw new InvalidOperationException("Источник данных СЭКОП не задан.");

                SekopValues values = provider();
                string hexPacket = await _service.SendAsync(
                    values.Transactions, values.Passengers);

                _logManager.AppendLog($"Протокол СЭКОП отправлен HEX: {hexPacket}");
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public void Stop()
        {
            lock (_stateLock)
            {
                if (!_running)
                    return;

                _running = false;
                _getValues = null;
                ++_sessionId;
            }

            // An in-progress connect/send is aborted; any queued send skips
            // the operation after taking _sendLock and checking _sessionId.
            _service.Disconnect();
            _logManager.AppendLog("Протокол СЭКОП остановлен");
        }
    }
}
