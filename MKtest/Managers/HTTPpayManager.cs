using MKtest.Configs;
using MKtest.Services.HTTPpay;
using MKtest.Services.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    public sealed class HTTPpayManager : IHttpPayOperations
    {
        private readonly HTTPpayService _service;
        private readonly LogManager _logManager;
        private readonly PeriodicOperationRunner _runner = new();
        private Func<int>? _readPtotal;

        public bool IsRunning => _runner.IsRunning;

        public HTTPpayManager(HTTPpayService service, LogManager logManager)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
        }

        public void Start(Func<int> getPtotal, int intervalSeconds)
        {
            ArgumentNullException.ThrowIfNull(getPtotal);
            if (intervalSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds), "Интервал должен быть больше 0.");
            if (IsRunning) return;

            _readPtotal = getPtotal;
            _runner.Start(
                SendOnceAsync,
                () => intervalSeconds,
                ex => _logManager.AppendLog($"HTTPpay: {ex.Message}"));
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                _runner.Stop();
                return;
            }
            _runner.Stop();
            _logManager.AppendLog("HTTPpay остановлен");
        }

        /// <summary>Await canceled HTTP/TCP operations before closing their transports.</summary>
        public Task StopAsync()
        {
            Stop();
            return _runner.StopAsync();
        }

        private async Task SendOnceAsync(CancellationToken token)
        {
            var getPtotal = _readPtotal;
            int total = getPtotal?.Invoke() ?? 0;
            HTTPpayResponse response = await _service.SendAsync(total, token);
            token.ThrowIfCancellationRequested();
            _logManager.AppendLog($"HTTPpay: {response.Message}");
        }
    }
}
