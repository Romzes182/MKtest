using MKtest.Managers;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKtest.UI.Beelink
{
    /// <summary>
    /// Coordinates the SSH Beelink time controls without owning the SSH
    /// transport or knowing anything about the containing MainForm.
    /// </summary>
    public sealed class BeelinkTimeUiController : IDisposable
    {
        private readonly TimeCommandsManager _operations;
        private readonly Func<bool> _isConnected;
        private readonly Func<bool> _isClosing;
        private readonly GroupBox _timeGroup;
        private readonly Button _disconnectButton;
        private readonly TextBox _remoteTimeTextBox;
        private readonly Action<string> _log;

        private bool _busy;
        private bool _disposed;

        public BeelinkTimeUiController(
            TimeCommandsManager operations,
            Func<bool> isConnected,
            Func<bool> isClosing,
            GroupBox timeGroup,
            Button disconnectButton,
            TextBox remoteTimeTextBox,
            Action<string> log)
        {
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            _isConnected = isConnected ?? throw new ArgumentNullException(nameof(isConnected));
            _isClosing = isClosing ?? throw new ArgumentNullException(nameof(isClosing));
            _timeGroup = timeGroup ?? throw new ArgumentNullException(nameof(timeGroup));
            _disconnectButton = disconnectButton ?? throw new ArgumentNullException(nameof(disconnectButton));
            _remoteTimeTextBox = remoteTimeTextBox ?? throw new ArgumentNullException(nameof(remoteTimeTextBox));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Task CheckAsync() =>
            ExecuteAsync(_operations.CheckTimeStatusAndReadTimeAsync, "Получение...");

        public Task EnableNtpAsync() =>
            ExecuteAsync(_operations.EnableNTPAndReadTimeAsync, "Включение NTP...");

        public Task DisableNtpAsync() =>
            ExecuteAsync(_operations.DisableNTPAndReadTimeAsync, "Отключение NTP...");

        public Task SetManualTimeAsync(DateTime date, DateTime time) =>
            ExecuteAsync(() => _operations.SetManualDateTimeAndReadTimeAsync(date, time),
                "Установка времени...");

        public void ClearReadout()
        {
            if (!_disposed && !_remoteTimeTextBox.IsDisposed)
                _remoteTimeTextBox.Text = "—";
        }

        private async Task ExecuteAsync(Func<Task<string?>> command, string progress)
        {
            if (_disposed || _busy || _isClosing())
                return;

            if (!_isConnected())
            {
                _remoteTimeTextBox.Text = "Нет подключения";
                _operations.EnsureConnected();
                return;
            }

            _busy = true;
            _timeGroup.Enabled = false;
            _disconnectButton.Enabled = false;
            _remoteTimeTextBox.Text = progress;

            try
            {
                string? receivedTime = await command();
                if (CanUpdateReadout())
                    _remoteTimeTextBox.Text = receivedTime ?? "Нет данных";
            }
            catch (Exception ex)
            {
                if (CanUpdateReadout())
                    _remoteTimeTextBox.Text = "Ошибка выполнения";

                _log($"Ошибка команды времени Beelink: {ex.Message}");
            }
            finally
            {
                _busy = false;
                if (!_disposed && !_isClosing() && !_timeGroup.IsDisposed)
                {
                    bool connected = _isConnected();
                    _timeGroup.Enabled = connected;
                    if (!_disconnectButton.IsDisposed)
                        _disconnectButton.Enabled = connected;
                }
            }
        }

        private bool CanUpdateReadout() =>
            !_disposed && !_isClosing() && !_remoteTimeTextBox.IsDisposed;

        public void Dispose() => _disposed = true;
    }
}
