using System;
using System.IO;
using System.Net;
using System.Text;

namespace MKtest.Services.WebServer
{
    /// <summary>
    /// Serves existing JsonTemplates files without knowing how the listener is started.
    /// Only files inside the configured directory are accessible.
    /// </summary>
    public sealed class StaticFileRequestHandler : IWebServerRequestHandler
    {
        private readonly string _rootPath;
        private readonly string _rootPrefix;

        public event EventHandler<string>? RequestLog;

        public StaticFileRequestHandler(string rootPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);

            _rootPath = Path.GetFullPath(rootPath);
            Directory.CreateDirectory(_rootPath);
            _rootPrefix = Path.EndsInDirectorySeparator(_rootPath)
                ? _rootPath
                : _rootPath + Path.DirectorySeparatorChar;
        }

        public void Handle(HttpListenerContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            var response = context.Response;

            try
            {
                var request = context.Request;
                var requestedFile = request.Url?.AbsolutePath.TrimStart('/') ?? string.Empty;
                Log($"HTTP запрос: {request.HttpMethod} {request.Url?.AbsolutePath}");

                // FullPath + a directory-boundary-aware check also rejects sibling
                // paths such as JsonTemplatesBackup and rooted request paths.
                var relativePath = requestedFile.Replace('/', Path.DirectorySeparatorChar);
                var safePath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));

                if (!IsWithinRoot(safePath))
                {
                    WriteMessage(response, 403, "Forbidden");
                    return;
                }

                if (!File.Exists(safePath))
                {
                    WriteMessage(response, 404, "File not found");
                    Log($"Файл не найден: {requestedFile}");
                    return;
                }

                var extension = Path.GetExtension(safePath).ToLowerInvariant();
                response.ContentType = extension switch
                {
                    ".json" => "application/json",
                    ".html" => "text/html",
                    ".txt" => "text/plain",
                    _ => "application/octet-stream"
                };

                response.ContentEncoding = Encoding.UTF8;
                var fileBytes = File.ReadAllBytes(safePath);
                response.StatusCode = 200;
                response.ContentLength64 = fileBytes.Length;
                response.OutputStream.Write(fileBytes, 0, fileBytes.Length);

                Log($"Отправлен файл: {requestedFile}");
            }
            catch (Exception ex)
            {
                Log($"Ошибка обработки HTTP запроса: {ex.Message}");
            }
            finally
            {
                try { response.Close(); }
                catch (HttpListenerException) { /* Клиент уже отключился. */ }
                catch (ObjectDisposedException) { /* Сервер останавливается. */ }
            }
        }

        private bool IsWithinRoot(string path) =>
            path.StartsWith(_rootPrefix, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(path, _rootPath, StringComparison.OrdinalIgnoreCase);

        private static void WriteMessage(HttpListenerResponse response, int status, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            response.StatusCode = status;
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
        }

        private void Log(string message) => RequestLog?.Invoke(this, message);
    }
}
