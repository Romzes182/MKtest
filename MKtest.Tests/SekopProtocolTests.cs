using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Services.SekopProtocol;

namespace MKtest.Tests;

[TestClass]
public sealed class SekopProtocolTests
{
    private static readonly byte[] ZeroPacket =
    {
        0x01, 0x0C, 0x00, 0x00, 0x00,
        0x34, 0x38, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x02
    };

    [TestMethod]
    public void BuildPacket_ZeroCounters_PreservesAll14Bytes()
    {
        byte[] packet = SekopPacketBuilder.BuildPacket(0, 0);
        Assert.HasCount(14, packet);
        CollectionAssert.AreEqual(ZeroPacket, packet);
    }

    [TestMethod]
    public void BuildPacket_NonzeroCounters_UsesCurrentLittleEndianWireOrder()
    {
        byte[] expected =
        {
            0x01, 0x0C, 0x34, 0x12, 0x07,
            0x34, 0x38, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x02
        };
        CollectionAssert.AreEqual(expected, SekopPacketBuilder.BuildPacket(0x1234, 7));
    }

    [TestMethod]
    public void BuildPacket_MaximumTwoByteTransactionsAndOneBytePassengers()
    {
        byte[] packet = SekopPacketBuilder.BuildPacket(65535, 255);
        Assert.AreEqual((byte)0xFF, packet[2]);
        Assert.AreEqual((byte)0xFF, packet[3]);
        Assert.AreEqual((byte)0xFF, packet[4]);
        CollectionAssert.AreEqual(ZeroPacket[5..], packet[5..]);
    }

    [TestMethod]
    [DataRow(256, 0, 1)]
    [DataRow(4660, 52, 18)]
    [DataRow(65535, 255, 255)]
    public void BuildPacket_TransactionBytePositions(int transactions, int low, int high)
    {
        byte[] packet = SekopPacketBuilder.BuildPacket(transactions, 0);
        Assert.AreEqual((byte)low, packet[2]);
        Assert.AreEqual((byte)high, packet[3]);
    }

    [TestMethod]
    public void HexEncoder_IsUppercaseTwoDigitSpaced()
    {
        const string expected = "01 0C 34 12 07 34 38 30 00 00 00 00 00 02";
        Assert.AreEqual(expected,
            SekopPacketBuilder.ToHexString(SekopPacketBuilder.BuildPacket(0x1234, 7)));
    }

    [TestMethod]
    public async Task ProtocolService_SendsExactEncodedBytesAndReturnsHex()
    {
        var transport = new RecordingTransport();
        var service = new SekopProtocolService(new SekopPacketEncoder(), transport);

        string hex = await service.SendAsync(0x1234, 7);

        Assert.AreEqual("01 0C 34 12 07 34 38 30 00 00 00 00 00 02", hex);
        Assert.IsNotNull(transport.Packet);
        CollectionAssert.AreEqual(
            SekopPacketBuilder.BuildPacket(0x1234, 7), transport.Packet);
        Assert.AreEqual(1, transport.SendCount);

        service.Disconnect();
        Assert.AreEqual(1, transport.DisconnectCount);
    }

    private sealed class RecordingTransport : ISekopPacketTransmitter
    {
        public byte[]? Packet { get; private set; }
        public int SendCount { get; private set; }
        public int DisconnectCount { get; private set; }

        public Task SendAsync(byte[] packet)
        {
            Packet = (byte[])packet.Clone();
            SendCount++;
            return Task.CompletedTask;
        }

        public void Disconnect() => DisconnectCount++;
    }
}
