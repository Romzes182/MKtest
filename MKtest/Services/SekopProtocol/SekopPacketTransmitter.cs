using MKtest.Configs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.SekopProtocol
{
    /// <summary>
    /// Одно TCP-подключение на пакет, как и в прежней реализации СЭКОП.
    /// Отмена сеанса запрещает отправку после завершения ConnectAsync.
    /// Очередность вызовов обеспечивает SekopProtocolManager.
    /// </summary>
    public sealed class SekopPacketTransmitter : ISekopPacketTransmitter
    {
        private readonly SekopProtocolConfig _config;
        private readonly ISekopPacketSenderService _sender;
        private int _generation;

        public SekopPacketTransmitter(
            SekopProtocolConfig config,
            ISekopPacketSenderService sender)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public async Task SendAsync(byte[] packet)
        {
            ArgumentNullException.ThrowIfNull(packet);
            var generation = Volatile.Read(ref _generation);

            try
            {
                await _sender.ConnectAsync(_config.IpAddress, _config.Port);

                // A Stop during connection must not send a stale packet.
                if (generation != Volatile.Read(ref _generation))
                    throw new OperationCanceledException("Передача СЭКОП остановлена.");

                await _sender.SendPacketAsync(packet);
            }
            finally
            {
                _sender.Disconnect();
            }
        }

        public void Disconnect()
        {
            Interlocked.Increment(ref _generation);
            _sender.Disconnect();
        }
    }
}
