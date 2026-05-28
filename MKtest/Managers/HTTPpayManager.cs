using MKtest.Configs;
using MKtest.Services.HTTPpay;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    public class HTTPpayManager
    {
        private readonly HTTPpayService _service;

        private readonly LogManager _logManager;

        private CancellationTokenSource? _cts;

        private bool _running;

        private Func<int>? _getPtotal;

        public bool IsRunning => _running;

        public HTTPpayManager(
            HTTPpayService service,
            LogManager logManager)
        {
            _service = service;

            _logManager = logManager;
        }

        public void Start(
            Func<int> getPtotal,
            int intervalSeconds)
        {
            if (_running)
                return;

            _running = true;

            _getPtotal = getPtotal;

            _cts = new CancellationTokenSource();

            _ = Task.Run(() =>
                LoopAsync(
                    intervalSeconds,
                    _cts.Token));
        }

        public void Stop()
        {
            _running = false;

            _cts?.Cancel();

            _logManager.AppendLog(
                "HTTPpay остановлен");
        }

        private async Task LoopAsync(
            int intervalSeconds,
            CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    int pTotal =
                        _getPtotal?.Invoke() ?? 0;

                    HTTPpayResponse response =
                        await _service.SendAsync(
                            pTotal);

                    _logManager.AppendLog(
                        $"HTTPpay: {response.Message}");
                }
                catch (Exception ex)
                {
                    _logManager.AppendLog(
                        ex.Message);
                }

                await Task.Delay(
                    intervalSeconds * 1000,
                    token);
            }
        }
    }
}