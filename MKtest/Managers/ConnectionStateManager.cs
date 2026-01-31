using System;
using System.Windows.Forms;

namespace MKtest.Managers
{
    public class ConnectionStateManager
    {
        private readonly Control _connectButton;
        private readonly Control _disconnectButton;
        private readonly Control _testButton;
        private readonly Control _statusLabel;
        private readonly Control[] _timeControls;

        public ConnectionStateManager(
            Button connectButton,
            Button disconnectButton,
            Button testButton,
            Label statusLabel,
            params Control[] timeControls)
        {
            _connectButton = connectButton;
            _disconnectButton = disconnectButton;
            _testButton = testButton;
            _statusLabel = statusLabel;
            _timeControls = timeControls;

            // Инициализируем начальное состояние при создании
            InitializeInitialState();
        }

        private void InitializeInitialState()
        {
            // Устанавливаем начальное состояние: не подключено
            UpdateButtonsState(false);
            UpdateStatusLabel("Статус: Не подключено");
        }

        public void UpdateConnectionButtons(bool isConnected)
        {
            if (_connectButton.InvokeRequired)
            {
                _connectButton.Invoke(new Action(() => UpdateButtonsState(isConnected)));
            }
            else
            {
                UpdateButtonsState(isConnected);
            }
        }

        public void UpdateStatusLabel(string status)
        {
            if (_statusLabel.InvokeRequired)
            {
                _statusLabel.Invoke(new Action(() => _statusLabel.Text = status));
            }
            else
            {
                _statusLabel.Text = status;
            }
        }

        public void SetButtonsEnabled(bool connectEnabled, bool disconnectEnabled, bool testEnabled)
        {
            SafeInvoke(_connectButton, () => _connectButton.Enabled = connectEnabled);
            SafeInvoke(_disconnectButton, () => _disconnectButton.Enabled = disconnectEnabled);
            SafeInvoke(_testButton, () => _testButton.Enabled = testEnabled);
        }

        private void UpdateButtonsState(bool isConnected)
        {
            _connectButton.Enabled = !isConnected;
            _disconnectButton.Enabled = isConnected;
            _testButton.Enabled = true;

            foreach (var control in _timeControls)
            {
                control.Enabled = isConnected;
            }
        }

        private void SafeInvoke(Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}