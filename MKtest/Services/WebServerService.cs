using MKtest.Managers;
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

    public class WebServerService : IWebServerService, IDisposable
    {
        private readonly LogManager? _logManager;
        private HttpListener? _listener;
        private Thread? _serverThread;
        private bool _isRunning;
        private string _serverUrl = string.Empty;
        private readonly string _rootPath;

        public bool IsRunning => _isRunning;
        public string ServerUrl => _serverUrl;

        public event EventHandler<string>? ServerLog;

        public WebServerService(LogManager? logManager = null)
        {
            _logManager = logManager;
            _rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
            Directory.CreateDirectory(_rootPath);
        }

        private static bool IsPortInUse(int port)
        {
            try
            {
                using var client = new TcpClient();
                var result = client.BeginConnect("127.0.0.1", port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                client.EndConnect(result);
                return success;
            }
            catch
            {
                return false;
            }
        }

        public void StartServer(string ipAddress = "*", int port = 8080)
        {
            try
            {
                if (IsPortInUse(port))
                {
                    var error = $"Ошибка: Порт {port} уже занят другим процессом";
                    OnServerLog(error);
                    return;
                }

                _isRunning = true;
                _listener = new HttpListener
                {
                    Prefixes = { $"http://{ipAddress}:{port}/" }
                };

                _serverUrl = $"http://{ipAddress}:{port}/";
                _serverThread = new Thread(Listen) { IsBackground = true };
                _serverThread.Start();

                var message = $"Веб-сервер запущен на {_serverUrl}";
                OnServerLog(message);

                string ipInfo = GetIPAddressInfo(port);
                OnServerLog(ipInfo);
            }
            catch (Exception ex)
            {
                var error = $"Ошибка запуска веб-сервера: {ex.Message}";
                OnServerLog(error);
            }
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
                    {
                        result.AppendLine($"http://{ip}:{port}/");
                    }
                }

                result.AppendLine($"http://localhost:{port}/");
                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"Ошибка получения IP-адресов: {ex.Message}";
            }
        }

        private void Listen()
        {
            try
            {
                var listener = _listener;
                if (listener == null) return;

                listener.Start();

                while (_isRunning)
                {
                    try
                    {
                        if (!listener.IsListening)
                            break;

                        var context = listener.GetContext();
                        Task.Run(() => ProcessRequest(context));
                    }
                    catch (HttpListenerException) when (!_isRunning)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
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

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                var requestedFile = request.Url?.AbsolutePath.TrimStart('/') ?? string.Empty;
                var logMessage = $"HTTP запрос: {request.HttpMethod} {request.Url?.AbsolutePath}";
                OnServerLog(logMessage);

                string safePath = Path.Combine(_rootPath, requestedFile);
                safePath = Path.GetFullPath(safePath);

                // Проверка безопасности
                if (!safePath.StartsWith(Path.GetFullPath(_rootPath)))
                {
                    response.StatusCode = 403;
                    var forbidden = Encoding.UTF8.GetBytes("Forbidden");
                    response.OutputStream.Write(forbidden, 0, forbidden.Length);
                    response.Close();
                    return;
                }

                if (File.Exists(safePath))
                {
                    var extension = Path.GetExtension(safePath).ToLower();
                    response.ContentType = extension switch
                    {
                        ".json" => "application/json",
                        ".html" => "text/html",
                        ".txt" => "text/plain",
                        _ => "application/octet-stream"
                    };

                    response.ContentEncoding = Encoding.UTF8;
                    var fileBytes = File.ReadAllBytes(safePath);
                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                    response.StatusCode = 200;
                    OnServerLog($"Отправлен файл: {requestedFile}");
                }
                else
                {
                    response.StatusCode = 404;
                    var notFound = Encoding.UTF8.GetBytes("File not found");
                    response.OutputStream.Write(notFound, 0, notFound.Length);
                    OnServerLog($"Файл не найден: {requestedFile}");
                }

                response.Close();
            }
            catch (Exception ex)
            {
                OnServerLog($"Ошибка обработки HTTP запроса: {ex.Message}");
            }
        }

        public void StopServer()
        {
            try
            {
                _isRunning = false;
                _listener?.Stop();
                _listener?.Close();
                _listener = null;

                _serverThread?.Join(1000);
                _serverThread = null;

                var message = "Веб-сервер остановлен";
                OnServerLog(message);
            }
            catch (Exception ex)
            {
                var error = $"Ошибка остановки веб-сервера: {ex.Message}";
                OnServerLog(error);
            }
        }

        protected virtual void OnServerLog(string message)
        {
            ServerLog?.Invoke(this, message);
            _logManager?.AppendLog($"[WebServer] {message}");
        }

        public void Dispose()
        {
            StopServer();
            _listener?.Close();
        }
    }
}