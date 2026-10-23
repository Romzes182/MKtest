namespace MKtest.Services.SekopProtocol
{
    /// <summary>Построение и форматирование пакетов СЭКОП без сетевого обмена.</summary>
    public interface ISekopPacketEncoder
    {
        byte[] Encode(int transactions, int passengers);
        string ToHexString(byte[] packet);
    }
}
