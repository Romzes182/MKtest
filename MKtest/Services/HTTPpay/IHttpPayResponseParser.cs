using MKtest.Configs;

namespace MKtest.Services.HTTPpay
{
    public interface IHttpPayResponseParser
    {
        HTTPpayResponse Parse(string rawResponse);
    }
}
