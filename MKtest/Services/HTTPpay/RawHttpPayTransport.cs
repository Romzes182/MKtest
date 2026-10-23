using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.HTTPpay
{
    /// <summary>
    /// Preserves the original raw HTTP/1.1 TCP GET used by the media system.
    /// Uses a linked cancellation token and the existing 30-second timeout.
    /// </summary>
    public sealed class RawHttpPayTransport : IHttpPayTransport
    {
        public async Task<string> GetRawAsync(string url, CancellationToken token)
        {
            var uri = new Uri(url);
            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("HTTPpay поддерживает только HTTP-запросы");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(30));
            using var client = new TcpClient();
            await client.ConnectAsync(uri.Host, uri.Port, cts.Token);
            await using var stream = client.GetStream();

            string pathAndQuery = string.IsNullOrEmpty(uri.PathAndQuery)
                ? "/" : uri.PathAndQuery;

            string request =
                $"GET {pathAndQuery} HTTP/1.1\r\n" +
                $"Host: {uri.Host}:{uri.Port}\r\n" +
                "Connection: close\r\n" +
                "Accept: */*\r\n\r\n";

            byte[] bytes = Encoding.ASCII.GetBytes(request);
            await stream.WriteAsync(bytes.AsMemory(), cts.Token);

            using var response = new MemoryStream();
            byte[] buffer = new byte[4096];
            while (true)
            {
                int count = await stream.ReadAsync(buffer.AsMemory(), cts.Token);
                if (count == 0) break;
                response.Write(buffer, 0, count);
            }

            return Encoding.UTF8.GetString(response.ToArray());
        }
    }
}
