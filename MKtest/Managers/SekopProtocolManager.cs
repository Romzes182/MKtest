using MKtest.Services.SekopProtocol;
using System;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    public readonly record struct SekopValues(
        int Transactions,
        int Passengers);

    public class SekopProtocolManager
    {
        private readonly SekopProtocolService _service;
        private readonly LogManager _logManager;

        private Func<SekopValues>? _getValues;

        private bool _running;

        public bool IsRunning => _running;

        public SekopProtocolManager(
            SekopProtocolService service,
            LogManager logManager)
        {
            _service = service;
            _logManager = logManager;
        }

        public async Task StartAsync(
            Func<SekopValues> getValues)
        {
            if (_running)
                return;

            _getValues = getValues;
            _running = true;

            await SendCurrentAsync();
        }

        public async Task SendCurrentAsync()
        {
            if (_getValues == null)
                throw new InvalidOperationException(
                    "Источник данных СЭКОП не задан.");

            SekopValues values = _getValues.Invoke();

            string hexPacket = await _service.SendAsync(
                values.Transactions,
                values.Passengers);

            _logManager.AppendLog(
                $"Протокол СЭКОП отправлен HEX: {hexPacket}");
        }

        public void Stop()
        {
            _running = false;

            _service.Disconnect();

            _logManager.AppendLog(
                "Протокол СЭКОП остановлен");
        }
    }
}