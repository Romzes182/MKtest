namespace MKtest.Services.SekopProtocol
{
    /// <summary>
    /// Адаптер текущего построителя: порядок и содержимое всех 14 байтов не меняются.
    /// </summary>
    public sealed class SekopPacketEncoder : ISekopPacketEncoder
    {
        public byte[] Encode(int transactions, int passengers) =>
            SekopPacketBuilder.BuildPacket(transactions, passengers);

        public string ToHexString(byte[] packet) =>
            SekopPacketBuilder.ToHexString(packet);
    }
}
