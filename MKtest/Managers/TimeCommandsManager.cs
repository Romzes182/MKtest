using MKtest.Services;
using MKtest.Services.SSH;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKtest.Managers
{
    /// <summary>
    /// Serializes commands to the one interactive Beelink SSH shell.
    /// The WinForms thread remains available while the remote device responds.
    /// </summary>
    public sealed class TimeCommandsManager
    {
        private readonly TimeCommandsService _timeService;
        private readonly LogManager _logManager;
        private readonly Func<bool> _checkConnection;
        private readonly SemaphoreSlim _commandGate = new(1, 1);

        public TimeCommandsManager(
            TimeCommandsService timeService,
            LogManager logManager,
            Func<bool> checkConnection)
        {
            _timeService = timeService ?? throw new ArgumentNullException(nameof(timeService));
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
            _checkConnection = checkConnection ?? throw new ArgumentNullException(nameof(checkConnection));
            _timeService.OnCommandExecuted += OnCommandExecuted;
        }

        private void OnCommandExecuted(string message) => _logManager.AppendLog(message);

        private bool CheckConnection()
        {
            if (_checkConnection())
                return true;

            MessageBox.Show("Сначала установите SSH подключение",
                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        /// <summary>Show the existing connection warning on the UI thread.</summary>
        public bool EnsureConnected() => CheckConnection();

        public Task<string?> CheckTimeStatusAndReadTimeAsync() =>
            ExecuteAndReadTimeAsync(null);

        public Task<string?> EnableNTPAndReadTimeAsync() =>
            ExecuteAndReadTimeAsync(_timeService.EnableNTP);

        public Task<string?> DisableNTPAndReadTimeAsync() =>
            ExecuteAndReadTimeAsync(_timeService.DisableNTP);

        public Task<string?> SetManualDateTimeAndReadTimeAsync(DateTime date, DateTime time)
        {
            string timestamp = $"{date:yyyy-MM-dd} {time:HH:mm:ss}";
            return ExecuteAndReadTimeAsync(
                () => _timeService.SetManualDateTime(timestamp));
        }

        private async Task<string?> ExecuteAndReadTimeAsync(Func<string>? timeChange)
        {
            // The warning must be shown only on the UI thread, not Task.Run.
            if (!CheckConnection())
                return null;

            await _commandGate.WaitAsync().ConfigureAwait(false);
            try
            {
                // Both modifying the clock and querying it happen in the same
                // serialized work item. No other time command can interleave.
                return await Task.Run(() =>
                {
                    if (!_checkConnection())
                    {
                        _logManager.AppendLog("SSH Beelink отключён до выполнения команды времени");
                        return null;
                    }

                    if (timeChange != null)
                    {
                        string response = timeChange();
                        _logManager.AppendLog(response);
                        if (response.TrimStart().StartsWith("ERROR:",
                                StringComparison.OrdinalIgnoreCase))
                            throw new InvalidOperationException(
                                "Команда времени Beelink завершилась с ошибкой: " + response);
                    }

                    string status = _timeService.CheckTimeStatus();
                    _logManager.AppendLog($"Статус времени:\n{status}");

                    string? remoteTime = BeelinkTimeValueParser.ExtractFromTimedatectl(status);
                    if (remoteTime == null)
                    {
                        _logManager.AppendLog("Локальное время в timedatectl не найдено, запрос date...");
                        string rawTime = _timeService.ReadCurrentTime();
                        remoteTime = BeelinkTimeValueParser.Extract(rawTime);

                        if (remoteTime == null)
                            _logManager.AppendLog(
                                $"Время Beelink не распознано. Ответ date: {rawTime}");
                    }

                    if (remoteTime != null)
                        _logManager.AppendLog($"Получено локальное время Beelink: {remoteTime}");

                    return remoteTime;
                }).ConfigureAwait(false);
            }
            finally
            {
                _commandGate.Release();
            }
        }

        /// <summary>
        /// Drain an in-flight time command before the SSH client is disposed
        /// by the application's shutdown coordinator.
        /// </summary>
        public async Task WaitForIdleAsync()
        {
            await _commandGate.WaitAsync().ConfigureAwait(false);
            _commandGate.Release();
        }
    }
}
