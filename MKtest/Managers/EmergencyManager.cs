using MKtest.Configs;
using MKtest.Services;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    public class EmergencyManager
    {
        private readonly EmergencyService _service;
        private readonly EmergencyConfig _config;

        public EmergencyManager(EmergencyService service,EmergencyConfig config)
        {
            _service = service;
            _config = config;
        }

        public async Task SendCommand1Async()
        {
            await _service.SendCommandAsync(_config.Command1Url,"Команда 1");
        }

        public async Task SendCommand2Async()
        {
            await _service.SendCommandAsync(_config.Command2Url,"Команда 2");
        }

        public async Task SendCommand3Async()
        {
            await _service.SendCommandAsync(_config.Command3Url,"Команда 3");
        }
        public async Task SendCommand4Async()
        {
            await _service.SendCommandAsync(_config.Command4Url, "Команда 4");
        }
    }
}