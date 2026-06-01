using System.Linq;
using System.Text;

namespace MKtest.Services.SekopProtocol
{
    public static class SekopPacketBuilder
    {
        public static byte[] BuildPacket(int transactions, int passengers)
        {
            byte[] packet =
            {
                0x01, 0x0C, // ТЭГ и длина

                0x00, 0x00, // транзакции, заменяются ниже
                0x00,       // пассажиры, заменяются ниже

                0x34, 0x38, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, // маршрут
                0x02        // рейс
            };

            // ВАЖНО: порядок байтов little-endian.
            // Сначала младший байт, потом старший.
            packet[2] = (byte)(transactions & 0xFF);
            packet[3] = (byte)((transactions >> 8) & 0xFF);

            // Пассажиры — 1 байт.
            packet[4] = (byte)passengers;

            return packet;
        }

        public static string ToHexString(byte[] packet)
        {
            return string.Join(" ", packet.Select(b => b.ToString("X2")));
        }

        public static string ToAsciiString(byte[] packet)
        {
            return Encoding.ASCII.GetString(packet);
        }
    }
}