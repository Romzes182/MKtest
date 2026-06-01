using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MKtest.Services.SekopProtocol
{
    public class SekopPacketSenderService : ISekopPacketSenderService, IDisposable
    {
        private TcpClient? _client;
        private NetworkStream? _stream;

        public bool IsConnected => _client?.Connected ?? false;

        public async Task ConnectAsync(string ip, int port)
        {
            _client = new TcpClient();

            await _client.ConnectAsync(ip, port);

            _stream = _client.GetStream();
        }

        public async Task SendPacketAsync(byte[] packet)
        {
            if (_stream == null || _client == null || !_client.Connected)
                throw new InvalidOperationException("Не подключено к серверу.");

            await _stream.WriteAsync(packet, 0, packet.Length);

            // Важно: сбрасываем буфер для немедленной отправки.
            await _stream.FlushAsync();
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();

            _stream = null;
            _client = null;
        }

        public void Dispose()
        {
            Disconnect();

            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}