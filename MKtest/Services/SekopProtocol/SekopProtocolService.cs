using MKtest.Configs;
using System.Threading.Tasks;

namespace MKtest.Services.SekopProtocol
{
    public class SekopProtocolService
    {
        private readonly SekopProtocolConfig _config;
        private readonly ISekopPacketSenderService _sender;

        public SekopProtocolService(
            SekopProtocolConfig config,
            ISekopPacketSenderService sender)
        {
            _config = config;
            _sender = sender;
        }

        public async Task<string> SendAsync(
            int transactions,
            int passengers)
        {
            byte[] packet = SekopPacketBuilder.BuildPacket(
                transactions,
                passengers);

            if (!_sender.IsConnected)
            {
                await _sender.ConnectAsync(
                    _config.IpAddress,
                    _config.Port);
            }

            await _sender.SendPacketAsync(packet);

            return SekopPacketBuilder.ToHexString(packet);
        }

        public void Disconnect()
        {
            _sender.Disconnect();
        }
    }
}