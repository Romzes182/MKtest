using MKtest.Configs;
using System;
using System.Net;

namespace MKtest.Services.HTTPpay
{
    /// <summary>Preserves existing HTTPpay status messages and response interpretation.</summary>
    public sealed class HttpPayResponseParser : IHttpPayResponseParser
    {
        public HTTPpayResponse Parse(string rawResponse)
        {
            int end = rawResponse.IndexOf("\r\n", StringComparison.Ordinal);
            string statusLine = end >= 0 ? rawResponse[..end] : rawResponse;
            var parts = statusLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int statusCode = 0;
            if (parts.Length >= 2)
                int.TryParse(parts[1], out statusCode);

            if (statusCode == 0)
                return new HTTPpayResponse
                {
                    Success = false,
                    Message = "Некорректный HTTP-ответ",
                    StatusCode = 0,
                    Timestamp = DateTime.Now
                };

            var status = (HttpStatusCode)statusCode;
            var response = new HTTPpayResponse
            {
                StatusCode = statusCode,
                Timestamp = DateTime.Now,
                Success = status == HttpStatusCode.OK
            };

            response.Message = status switch
            {
                HttpStatusCode.OK => "OK - Запрос успешно обработан",
                HttpStatusCode.BadRequest => "400 - Ошибка в формате данных",
                HttpStatusCode.NotFound => "404 - Страница не найдена",
                HttpStatusCode.InternalServerError => "500 - Внутренняя ошибка сервера",
                _ => $"{statusCode} - Ошибка"
            };

            return response;
        }
    }
}
