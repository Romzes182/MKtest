using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.HTTPpay
{
    /// <summary>Low-level HTTP transport, independent from request and response models.</summary>
    public interface IHttpPayTransport
    {
        Task<string> GetRawAsync(string url, CancellationToken token);
    }
}
