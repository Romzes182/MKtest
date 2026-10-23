using System;
using System.Net;

namespace MKtest.Services.WebServer
{
    /// <summary>
    /// HTTP listener delegates each accepted request to a handler.
    /// New request behavior can be introduced without changing listener lifecycle.
    /// </summary>
    public interface IWebServerRequestHandler
    {
        event EventHandler<string>? RequestLog;
        void Handle(HttpListenerContext context);
    }
}
