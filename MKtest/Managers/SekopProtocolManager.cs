using MKtest.Services.SekopProtocol;
using System;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    public class SekopProtocolManager
    {
        private readonly SekopProtocolService _service;
        private readonly LogManager _logManager;

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
            int transactions,
            int passengers)
        {
            if (_running)
                return;

            _running = true;

            try
            {
                string hexPacket = await _service.SendAsync(
                    transactions,
                    passengers);

                _logManager.AppendLog(
                    $"Протокол СЭКОП отправлен HEX: {hexPacket}");
            }
            catch (Exception ex)
            {
                _running = false;

                _logManager.AppendLog(
                    $"Ошибка протокола СЭКОП: {ex.Message}");

                throw;
            }
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