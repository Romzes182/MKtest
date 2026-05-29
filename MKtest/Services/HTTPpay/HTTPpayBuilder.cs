using MKtest.Configs;
using System;
using System.Net;

namespace MKtest.Services.HTTPpay
{
    public static class HTTPpayBuilder
    {
        public static string BuildUrl(
        HTTPpayConfig config,
        int pTotal)
        {
            var tripDate =
                WebUtility.UrlEncode(
                    config.TripDate.ToString("yyyy-MM-dd HH:mm:ss"));

            var url =
                $"http://{config.IP}:{config.Port}/payments?" +
                $"terminal={WebUtility.UrlEncode(config.Terminal)}&" +
                $"route={WebUtility.UrlEncode(config.Route)}&" +
                $"trip={config.Trip}&" +
                $"tripDate={tripDate}&" +
                $"pTotal={pTotal}&" +
                $"pCurrent={config.CurrentPayments}";

            if (config.IncludeTimestamp)
            {
                url +=
                    $"&timestamp={WebUtility.UrlEncode(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))}";
            }

            return url;
        }
    }
}