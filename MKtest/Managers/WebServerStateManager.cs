using System.Windows.Forms;

namespace MKtest.Managers
{
    /// <summary>Only the visual state of the web-server controls.</summary>
    public sealed class WebServerStateManager
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
            _startButton.Enabled = true;
            _stopButton.Enabled = false;
            _statusLabel.Text = $"Статус: Ошибка - {error}";
        }
    }
}
