using MKtest.Configs;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.HTTPpay
{
    /// <summary>HTTPpay coordination only; raw wire format and parsing live separately.</summary>
    public sealed class HTTPpayService : IDisposable
    {
        private readonly HTTPpayConfig _config;
        private readonly IHttpPayUrlBuilder _builder;
        private readonly IHttpPayTransport _transport;
        private readonly IHttpPayResponseParser _parser;

        public HTTPpayService(HTTPpayConfig config)
            : this(config, new HttpPayUrlBuilder(), new RawHttpPayTransport(), new HttpPayResponseParser()) { }

        public HTTPpayService(
            HTTPpayConfig config,
            IHttpPayUrlBuilder builder,
            IHttpPayTransport transport,
            IHttpPayResponseParser parser)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public Task<HTTPpayResponse> SendAsync(int pTotal) =>
            SendAsync(pTotal, CancellationToken.None);

        public async Task<HTTPpayResponse> SendAsync(int pTotal, CancellationToken token)
        {
            try
            {
                string url = _builder.BuildUrl(_config, pTotal);
                string rawResponse = await _transport.GetRawAsync(url, token);
                return _parser.Parse(rawResponse);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                return Failed("Таймаут запроса");
            }
            catch (Exception ex) when (
                ex is HttpRequestException or IOException or SocketException
                    or UriFormatException or InvalidOperationException)
            {
                return Failed($"Ошибка сети: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Failed($"Ошибка: {ex.Message}");
            }
        }

        private static HTTPpayResponse Failed(string message) => new()
        {
            Success = false,
            Message = message,
            StatusCode = 0,
            Timestamp = DateTime.Now
        };

        // Kept for compatibility with MainForm's shutdown sequence.
        public void Dispose() { }
    }
}
