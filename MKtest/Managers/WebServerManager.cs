using MKtest.Configs;
using MKtest.Services;
using System;
using System.Windows.Forms;

namespace MKtest.Managers
{
    public class WebServerManager
    {
        private readonly IWebServerService _webServerService;
        private readonly LogManager _logManager;
        private readonly WebServerStateManager _stateManager;

        public WebServerManager(IWebServerService webServerService, LogManager logManager, WebServerStateManager stateManager)
        {
            _webServerService = webServerService ?? throw new ArgumentNullException(nameof(webServerService));
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));

            _webServerService.ServerLog += WebServerService_ServerLog;
        }

        private void WebServerService_ServerLog(object? sender, string e)
        {
            _logManager.AppendLog($"[WebServer] {e}");
        }

        public void StartServer(WebServerConfig config)
        {
            _webServerService.StartServer(config.IpAddress, config.Port);
            _stateManager.SetRunningState();
        }

        public void StopServer()
        {
            _webServerService.StopServer();
            _stateManager.SetStoppedState();
        }

        public bool IsRunning()
        {
            return _webServerService.IsRunning;
        }
    }

    public class WebServerStateManager
    {
        private readonly Button _startButton;
        private readonly Button _stopButton;
        private readonly Label _statusLabel;

        public WebServerStateManager(Button startButton, Button stopButton, Label statusLabel)
        {
            _startButton = startButton;
            _stopButton = stopButton;
            _statusLabel = statusLabel;
            SetStoppedState();
        }

        public void SetRunningState()
        {
            _startButton.Enabled = false;
            _stopButton.Enabled = true;
            _statusLabel.Text = "Статус: Запущен";
        }

        public void SetStoppedState()
        {
            _startButton.Enabled = true;
            _stopButton.Enabled = false;
            _statusLabel.Text = "Статус: Остановлен";
        }

        public void SetErrorState(string error)
        {
            _statusLabel.Text = $"Статус: Ошибка - {error}";
        }
    }
}