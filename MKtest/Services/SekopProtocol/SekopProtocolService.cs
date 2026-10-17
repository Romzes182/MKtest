using MKtest.Configs;
using System;
using System.Threading.Tasks;

namespace MKtest.Services.SekopProtocol
{
    /// <summary>
    /// Превращает значения СЭКОП в пакет и передаёт его транспортному слою.
    /// Не знает о кнопках, полях ввода и TCP-клиенте.
    /// </summary>
    public sealed class SekopProtocolService
    {
        private readonly ISekopPacketEncoder _encoder;
        private readonly ISekopPacketTransmitter _transmitter;

        // Совместимость с текущей сборкой в MainForm.
        public SekopProtocolService(
            SekopProtocolConfig config,
            ISekopPacketSenderService sender)
            : this(new SekopPacketEncoder(),
                   new SekopPacketTransmitter(config, sender))
        {
        }

        public SekopProtocolService(
            ISekopPacketEncoder encoder,
            ISekopPacketTransmitter transmitter)
        {
            _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
            _transmitter = transmitter ?? throw new ArgumentNullException(nameof(transmitter));
        }

        public async Task<string> SendAsync(int transactions, int passengers)
        {
            byte[] packet = _encoder.Encode(transactions, passengers);
            await _transmitter.SendAsync(packet);
            return _encoder.ToHexString(packet);
        }

        public void Disconnect() => _transmitter.Disconnect();
    }
}
