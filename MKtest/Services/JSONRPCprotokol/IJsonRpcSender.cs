using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.JSONRPCprotokol
{
    public interface IJsonRpcSender
    {
        Task SendAsync(int transactions, CancellationToken token);
    }
}
