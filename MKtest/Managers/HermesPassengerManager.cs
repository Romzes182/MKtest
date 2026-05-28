using MKtest.Managers;
using MKtest.Services.HermesPassenger;
using System;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    public class HermesPassengerManager
    {
        private readonly IHermesPassengerService _service;
        private readonly LogManager _log;

        public event Action<string>? StatusChanged;
        public bool IsRunning => _service.IsRunning;

        public HermesPassengerManager(IHermesPassengerService service, LogManager log)
        {
            _service = service;
            _log = log;
            _service.StatusChanged += s => StatusChanged?.Invoke(s);
            _service.ErrorOccurred += e => _log.AppendLog($"Гермес ошибка: {e}");
        }

        public Task<bool> TestConnectionAsync() => _service.TestConnectionAsync();
        public Task StartAsync(int entered, int exited) => _service.StartAsync(entered, exited);
        public Task StopAsync() => _service.StopAsync();
        public void UpdateValues(int entered, int exited) => _service.SetValues(entered, exited);
    }
}