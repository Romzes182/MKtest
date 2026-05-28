using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.JSONRPCprotokol
{
    public static class JSONRPCprotokolSender
    {
        public static async Task SendAsync(
            string ip,
            int port,
            string json,
            CancellationToken token)
        {
            using var client = new TcpClient();

            await client.ConnectAsync(ip, port, token);

            using var stream = client.GetStream();

            byte[] data = Encoding.UTF8.GetBytes(json);

            await stream.WriteAsync(data, token);

            await stream.FlushAsync(token);
        }
    }
}