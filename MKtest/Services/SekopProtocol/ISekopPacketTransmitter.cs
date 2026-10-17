using System.Threading.Tasks;

namespace MKtest.Services.SekopProtocol
{
    /// <summary>Отправка готового массива байтов независимо от его формата.</summary>
    public interface ISekopPacketTransmitter
    {
        Task SendAsync(byte[] packet);
        void Disconnect();
    }
}
