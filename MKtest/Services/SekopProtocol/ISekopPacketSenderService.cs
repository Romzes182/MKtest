using System.Threading.Tasks;

namespace MKtest.Services.SekopProtocol
{
    public interface ISekopPacketSenderService
    {
        Task ConnectAsync(string ip, int port);

        Task SendPacketAsync(byte[] packet);

        void Disconnect();

        bool IsConnected { get; }
    }
}