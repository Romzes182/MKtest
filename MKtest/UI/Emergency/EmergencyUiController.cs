using MKtest.Managers;
using System;
using System.Threading.Tasks;

namespace MKtest.UI.Emergency
{
    /// <summary>
    /// Routes MChS button actions to the existing operations.
    /// Doesn't know URLs or the HTTP implementation.
    /// </summary>
    public sealed class EmergencyUiController
    {
        private readonly IEmergencyOperations _operations;
        private readonly Action<string> _log;

        public EmergencyUiController(IEmergencyOperations operations, Action<string> log)
        {
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task SendAsync(int command)
        {
            try
            {
                switch (command)
                {
                    case 1:
                        await _operations.SendCommand1Async();
                        break;
                    case 2:
                        await _operations.SendCommand2Async();
                        break;
                    case 3:
                        await _operations.SendCommand3Async();
                        break;
                    case 4:
                        await _operations.SendCommand4Async();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(command));
                }
            }
            catch (Exception ex)
            {
                _log($"[МЧС] Ошибка команды {command}: {ex.Message}");
            }
        }
    }
}
