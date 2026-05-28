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
                    "2021-01-01 05:00:00");

            var timestamp =
                WebUtility.UrlEncode(
                    DateTime.Now.ToString(
                        "yyyy-MM-dd HH:mm:ss"));

            return
                $"http://{config.IP}:{config.Port}/payments?" +
                $"terminal=51000200030004&" +
                $"route=4000000000020016&" +
                $"trip=2&" +
                $"tripDate={tripDate}&" +
                $"pTotal={pTotal}&" +
                $"pCurrent=6&" +
                $"timestamp={timestamp}";
        }
    }
}