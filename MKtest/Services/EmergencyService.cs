using MKtest.Managers;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MKtest.Services
{
    public class EmergencyService
    {
        private readonly LogManager _logger;
        private readonly HttpClient _httpClient;

        public EmergencyService(LogManager logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async Task SendCommandAsync(
            string url,
            string commandName)
        {
            try
            {
                _logger.AppendLog(
                    $"[МЧС] Отправка команды: {commandName}");

                var response =
                    await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    _logger.AppendLog(
                        $"[МЧС] Команда {commandName} успешно отправлена");
                }
                else
                {
                    _logger.AppendLog(
                        $"[МЧС] Ошибка {commandName}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.AppendLog(
                    $"[МЧС] Ошибка {commandName}: {ex.Message}");
            }
        }
    }
}