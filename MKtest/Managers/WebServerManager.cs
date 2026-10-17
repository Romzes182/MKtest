using MKtest.Configs;
using MKtest.Services;
using System;

namespace MKtest.Managers
{
    /// <summary>
    /// Maps the IWebServerService state to WinForms, without handling HTTP requests.
    /// </summary>
    public sealed class WebServerManager
    {
        private readonly IWebServerService _webServerService;
        private readonly LogManager _logManager;
        private readonly WebServerStateManager _stateManager;

        public WebServerManager(
            IWebServerService webServerService,
            LogManager logManager,
            WebServerStateManager stateManager)
        {
            _webServerService = webServerService ?? throw new ArgumentNullException(nameof(webServerService));
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));

            _webServerService.ServerLog += WebServerService_ServerLog;
        }

        private void WebServerService_ServerLog(object? sender, string message) =>
            _logManager.AppendLog($"[WebServer] {message}");

        public void StartServer(WebServerConfig config)
        {
            _webServerService.StartServer(config.IpAddress, config.Port);

            if (_webServerService.IsRunning)
                _stateManager.SetRunningState();
            else
                _stateManager.SetErrorState("Не удалось запустить веб-сервер");
        }

        public void StopServer()
        {
            _webServerService.StopServer();
            _stateManager.SetStoppedState();
        }

        public bool IsRunning() => _webServerService.IsRunning;
    }
}
