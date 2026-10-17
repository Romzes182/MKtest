using MKtest.Managers;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services
{
    /// <summary>HTTP requests for the four configured MChS commands.</summary>
    public sealed class EmergencyService : IDisposable
    {
        private readonly LogManager _logger;
        private readonly HttpClient _httpClient;
        private readonly CancellationTokenSource _shutdown = new();
        private bool _disposed;

        public EmergencyService(LogManager logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient();
        }

        public async Task SendCommandAsync(string url, string commandName)
        {
            if (_disposed)
                return;

            try
            {
                _logger.AppendLog($"[МЧС] Отправка команды: {commandName}");

                using var response = await _httpClient.GetAsync(
                    url, _shutdown.Token).ConfigureAwait(false);

                if (_shutdown.IsCancellationRequested)
                    return;

                if (response.IsSuccessStatusCode)
                    _logger.AppendLog($"[МЧС] Команда {commandName} успешно отправлена");
                else
                    _logger.AppendLog($"[МЧС] Ошибка {commandName}: {response.StatusCode}");
            }
            catch (OperationCanceledException) when (_shutdown.IsCancellationRequested)
            {
                // Form closing: expected cancellation, not a protocol failure.
            }
            catch (ObjectDisposedException) when (_disposed)
            {
                // The HTTP client was disposed during shutdown.
            }
            catch (Exception ex)
            {
                if (!_disposed)
                    _logger.AppendLog($"[МЧС] Ошибка {commandName}: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _shutdown.Cancel();
            _httpClient.Dispose();
            // Keep the canceled token source alive: an in-flight continuation
            // can still inspect IsCancellationRequested after disposal.
        }
    }
}
