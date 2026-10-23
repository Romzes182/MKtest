using MKtest.Configs;

namespace MKtest.Services.HTTPpay
{
    public interface IHttpPayUrlBuilder
    {
        string BuildUrl(HTTPpayConfig config, int totalPayments);
    }

    public sealed class HttpPayUrlBuilder : IHttpPayUrlBuilder
    {
        public string BuildUrl(HTTPpayConfig config, int totalPayments) =>
            HTTPpayBuilder.BuildUrl(config, totalPayments);
    }
}
