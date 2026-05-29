using MKtest.Configs;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.HTTPpay
{
    public class HTTPpayService : IDisposable
    {
        private const int TimeoutSeconds = 30;

        private readonly HTTPpayConfig _config;

        private readonly HttpClient _httpClient;

        public HTTPpayService(
            HTTPpayConfig config)
        {
            _config = config;

            _httpClient = new HttpClient();

            _httpClient.Timeout =
                TimeSpan.FromSeconds(30);
        }

        public async Task<HTTPpayResponse> SendAsync(int pTotal)
        {
            try
            {
                var url =
                    HTTPpayBuilder.BuildUrl(
                        _config,
                        pTotal);

                var rawResponse =
                    await SendRawGetAsync(url);

                return ParseRawResponse(rawResponse);
            }
            catch (TaskCanceledException)
            {
                return new HTTPpayResponse
                {
                    Success = false,

                    Message = "Таймаут запроса",

                    StatusCode = 0,

                    Timestamp = DateTime.Now
                };
            }
            catch (Exception ex) when (
                ex is HttpRequestException ||
                ex is IOException ||
                ex is SocketException ||
                ex is UriFormatException ||
                ex is InvalidOperationException)
            {
                return new HTTPpayResponse
                {
                    Success = false,
                    Message =
                        $"Ошибка сети: {ex.Message}",

                    StatusCode = 0,

                    Timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                return new HTTPpayResponse
                {
                    Success = false,

                    Message =
                        $"Ошибка: {ex.Message}",

                    StatusCode = 0,

                    Timestamp = DateTime.Now
                };
            }
        }

        private static async Task<string> SendRawGetAsync(string url)
        {
            var uri = new Uri(url);

            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("HTTPpay поддерживает только HTTP-запросы");

            using var cts =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(TimeoutSeconds));

            using var client = new TcpClient();

            await client.ConnectAsync(
                uri.Host,
                uri.Port,
                cts.Token);

            await using var stream =
                client.GetStream();

            var pathAndQuery =
                string.IsNullOrEmpty(uri.PathAndQuery)
                    ? "/"
                    : uri.PathAndQuery;

            var request =
                $"GET {pathAndQuery} HTTP/1.1\r\n" +
                $"Host: {uri.Host}:{uri.Port}\r\n" +
                "Connection: close\r\n" +
                "Accept: */*\r\n" +
                "\r\n";

            var requestBytes =
                Encoding.ASCII.GetBytes(request);

            await stream.WriteAsync(
                requestBytes.AsMemory(0, requestBytes.Length),
                cts.Token);

            var buffer = new byte[4096];

            using var response =
                new MemoryStream();

            while (true)
            {
                var read =
                    await stream.ReadAsync(
                        buffer.AsMemory(0, buffer.Length),
                        cts.Token);

                if (read == 0)
                    break;

                response.Write(
                    buffer,
                    0,
                    read);
            }

            return Encoding.UTF8.GetString(
                response.ToArray());
        }

        private HTTPpayResponse ParseRawResponse(string rawResponse)
        {
            var firstLineEnd =
                rawResponse.IndexOf("\r\n", StringComparison.Ordinal);

            var statusLine =
                firstLineEnd >= 0
                    ? rawResponse.Substring(0, firstLineEnd)
                    : rawResponse;

            var statusCode = 0;

            var parts =
                statusLine.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
                int.TryParse(parts[1], out statusCode);

            if (statusCode == 0)
            {
                return new HTTPpayResponse
                {
                    Success = false,
                    Message = "Некорректный HTTP-ответ",
                    StatusCode = 0,
                    Timestamp = DateTime.Now
                };
            }

            return ParseResponse(
                (System.Net.HttpStatusCode)statusCode,
                rawResponse);
        }
        private HTTPpayResponse ParseResponse(System.Net.HttpStatusCode statusCode, string content)
        {
            var response =
                new HTTPpayResponse
                {
                    StatusCode = (int)statusCode,

                    Timestamp = DateTime.Now
                };

            switch (statusCode)
            {
                case System.Net.HttpStatusCode.OK:

                    response.Success = true;

                    response.Message =
                        "OK - Запрос успешно обработан";

                    break;

                case System.Net.HttpStatusCode.BadRequest:

                    response.Success = false;

                    response.Message =
                        "400 - Ошибка в формате данных";

                    break;

                case System.Net.HttpStatusCode.NotFound:

                    response.Success = false;

                    response.Message =
                        "404 - Страница не найдена";

                    break;

                case System.Net.HttpStatusCode.InternalServerError:

                    response.Success = false;

                    response.Message =
                        "500 - Внутренняя ошибка сервера";

                    break;

                default:

                    response.Success = false;

                    response.Message =
                        $"{(int)statusCode} - Ошибка";

                    break;
            }

            return response;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}