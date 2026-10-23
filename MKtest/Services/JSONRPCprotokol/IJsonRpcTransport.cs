using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.JSONRPCprotokol
{
    public interface IJsonRpcTransport
    {
        Task SendAsync(string ip, int port, string json, CancellationToken token);
    }

    public sealed class TcpJsonRpcTransport : IJsonRpcTransport
    {
        public Task SendAsync(string ip, int port, string json, CancellationToken token) =>
            JSONRPCprotokolSender.SendAsync(ip, port, json, token);
    }
}
