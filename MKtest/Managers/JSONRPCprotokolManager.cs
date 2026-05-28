using MKtest.Services.JSONRPCprotokol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    public class JSONRPCprotokolManager
    {
        private readonly JSONRPCprotokolService _service;
        private readonly LogManager _logManager;

        private CancellationTokenSource? _cts;

        private bool _running;

        private int _trCounter;
        private int _intervalSeconds;
        public bool IsRunning => _running;

        public JSONRPCprotokolManager(
            JSONRPCprotokolService service,
            LogManager logManager)
        {
            _service = service;
            _logManager = logManager;
        }

        public void Start(
            int trCounter,
            int intervalSeconds)
        {
            if (_running)
                return;

            _trCounter = trCounter;
            _intervalSeconds = intervalSeconds;

            _running = true;

            _cts = new CancellationTokenSource();

            _ = Task.Run(() =>
            LoopAsync(_cts.Token));
        }

        public void Stop()
        {
            _running = false;

            _cts?.Cancel();

            _logManager.AppendLog(
                "JSONRPCprotokol остановлен");
        }

        public void UpdateValues(
            int trCounter,
            int intervalSeconds)
        {
            _trCounter = trCounter;
            _intervalSeconds = intervalSeconds;

            _logManager.AppendLog(
                $"JSONRPCprotokol обновлен: trCounter={trCounter}, interval={intervalSeconds}");
        }

        private async Task LoopAsync(
            CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await _service.SendAsync(
                        _trCounter,
                        token);

                    _logManager.AppendLog(
                        $"JSONRPCprotokol отправлен: {_trCounter}");
                }
                catch (Exception ex)
                {
                    _logManager.AppendLog(ex.Message);
                }

                await Task.Delay(
                    _intervalSeconds * 1000,
                    token);
            }
        }
    }
}