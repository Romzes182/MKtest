using MKtest.Services.JSONRPCprotokol;
using MKtest.Services.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    public sealed class JSONRPCprotokolManager
    {
        private readonly JSONRPCprotokolService _service;
        private readonly LogManager _logManager;
        private readonly PeriodicOperationRunner _runner = new();
        private int _counter;
        private int _interval;

        public bool IsRunning => _runner.IsRunning;

        public JSONRPCprotokolManager(JSONRPCprotokolService service, LogManager logManager)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
        }

        public void Start(int trCounter, int intervalSeconds)
        {
            if (intervalSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds), "Интервал должен быть больше 0.");

            if (IsRunning) return;
            Interlocked.Exchange(ref _counter, trCounter);
            Interlocked.Exchange(ref _interval, intervalSeconds);
            _runner.Start(
                SendOnceAsync,
                () => Volatile.Read(ref _interval),
                ex => _logManager.AppendLog($"JSONRPCprotokol: {ex.Message}"));
        }

        public void Stop()
        {
            if (!IsRunning) return;
            _runner.Stop();
            _logManager.AppendLog("JSONRPCprotokol остановлен");
        }

        public void UpdateValues(int trCounter, int intervalSeconds)
        {
            if (intervalSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds), "Интервал должен быть больше 0.");

            Interlocked.Exchange(ref _counter, trCounter);
            Interlocked.Exchange(ref _interval, intervalSeconds);
            _logManager.AppendLog(
                $"JSONRPCprotokol обновлен: trCounter={trCounter}, interval={intervalSeconds}");
        }

        private async Task SendOnceAsync(CancellationToken token)
        {
            int current = Volatile.Read(ref _counter);
            await _service.SendAsync(current, token);
            _logManager.AppendLog($"JSONRPCprotokol отправлен: {current}");
        }
    }
}
