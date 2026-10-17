using MKtest.Services.WebServer;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services
{
    public interface IWebServerService
    {
        bool IsRunning { get; }
        string ServerUrl { get; }
        void StartServer(string ipAddress = "*", int port = 8080);
        void StopServer();
        event EventHandler<string> ServerLog;
    }

    /// <summary>
    /// Owns the HttpListener lifecycle; request details belong to IWebServerRequestHandler.
    /// No WinForms or LogManager dependencies.
    /// </summary>
    public sealed class WebServerService : IWebServerService, IDisposable
    {
        private readonly IWebServerRequestHandler _requestHandler;
        private readonly object _sync = new();
        private HttpListener? _listener;
        private Thread? _serverThread;
        private volatile bool _isRunning;
        private string _serverUrl = string.Empty;

        public bool IsRunning => _isRunning;
        public string ServerUrl => _serverUrl;

        public event EventHandler<string>? ServerLog;

        public WebServerService(IWebServerRequestHandler? requestHandler = null)
        {
            _requestHandler = requestHandler ??
                new StaticFileRequestHandler(Path.Combine(AppContext.BaseDirectory, "JsonTemplates"));
            _requestHandler.RequestLog += RequestHandlerLog;
        }

        public void StartServer(string ipAddress = "*", int port = 8080)
        {
            string? error = null;
            string? url = null;

            lock (_sync)
            {
                if (_isRunning)
                    return;

                HttpListener? newListener = null;
                try
                {
                    url = $"http://{ipAddress}:{port}/";
                    newListener = new HttpListener();
                    newListener.Prefixes.Add(url);

                    // Start synchronously: a failed bind must never appear as a running server.
                    newListener.Start();

                    _listener = newListener;
                    _serverUrl = url;
                    _isRunning = true;
                    _serverThread = new Thread(() => Listen(newListener))
                    {
                        IsBackground = true
                    };
                    _serverThread.Start();
                }
                catch (Exception ex)
                {
                    _isRunning = false;
                    _listener = null;
                    _serverThread = null;
                    newListener?.Close();
                    error = ex.Message;
                }
            }

            if (error != null)
            {
                OnServerLog($"Ошибка запуска веб-сервера: {error}");
                return;
            }

            OnServerLog($"Веб-сервер запущен на {url}");
            OnServerLog(GetIPAddressInfo(port));
        }

        private static string GetIPAddressInfo(int port)
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var result = new StringBuilder();
                result.AppendLine("Доступные адреса для подключения:");

                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        result.AppendLine($"http://{ip}:{port}/");
                }

                result.AppendLine($"http://localhost:{port}/");
                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Ошибка получения IP-адресов: {ex.Message}";
            }
        }

        private void Listen(HttpListener listener)
        {
            try
            {
                while (_isRunning)
                {
                    try
                    {
                        var context = listener.GetContext();
                        _ = Task.Run(() => _requestHandler.Handle(context));
                    }
                    catch (HttpListenerException) when (!_isRunning)
                    {
                        break;
                    }
                    catch (ObjectDisposedException) when (!_isRunning)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        OnServerLog($"Ошибка при обработке подключения: {ex.Message}");
                        if (!_isRunning) break;
                    }
                }
            }
            catch (Exception ex)
            {
                OnServerLog($"Ошибка в веб-сервере: {ex.Message}");
            }
        }

        public void StopServer()
        {
            HttpListener? listener;
            Thread? serverThread;

            lock (_sync)
            {
                if (!_isRunning && _listener == null)
                    return;

                _isRunning = false;
                listener = _listener;
                serverThread = _serverThread;
                _listener = null;
                _serverThread = null;
            }

            try
            {
                listener?.Stop();
                listener?.Close();
                if (serverThread != Thread.CurrentThread)
                    serverThread?.Join(1000);

                OnServerLog("Веб-сервер остановлен");
            }
            catch (Exception ex)
            {
                OnServerLog($"Ошибка остановки веб-сервера: {ex.Message}");
            }
        }

        private void RequestHandlerLog(object? sender, string message) => OnServerLog(message);

        private void OnServerLog(string message) => ServerLog?.Invoke(this, message);

        public void Dispose()
        {
            StopServer();
            _requestHandler.RequestLog -= RequestHandlerLog;
        }
    }
}
