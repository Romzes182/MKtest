using MKtest.Configs;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MKtest.Services.HTTPpay
{
    public class HTTPpayService : IDisposable
    {
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

        public async Task<HTTPpayResponse>
            SendAsync(int pTotal)
        {
            try
            {
                var url =
                    HTTPpayBuilder.BuildUrl(
                        _config,
                        pTotal);

                var response =
                    await _httpClient.GetAsync(url);

                var content =
                    await response.Content
                        .ReadAsStringAsync();

                return ParseResponse(
                    response.StatusCode,
                    content);
            }
            catch (HttpRequestException ex)
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

        private HTTPpayResponse ParseResponse(
            System.Net.HttpStatusCode statusCode,
            string content)
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