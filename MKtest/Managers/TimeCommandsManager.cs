using MKtest.Services;
using System;
using System.Windows.Forms;

namespace MKtest.Managers
{
    public class TimeCommandsManager
    {
        private readonly TimeCommandsService _timeService;
        private readonly LogManager _logManager;
        private readonly Func<bool> _checkConnection;

        public TimeCommandsManager(TimeCommandsService timeService, LogManager logManager, Func<bool> checkConnection)
        {
            _timeService = timeService;
            _logManager = logManager;
            _checkConnection = checkConnection;

            _timeService.OnCommandExecuted += OnCommandExecuted;
        }

        private void OnCommandExecuted(string message) => _logManager.AppendLog(message);

        private bool CheckConnection()
        {
            if (!_checkConnection())
            {
                MessageBox.Show("Сначала установите SSH подключение",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        public void CheckTimeStatus()
        {
            if (!CheckConnection()) return;
            var result = _timeService.CheckTimeStatus();
            _logManager.AppendLog($"Статус времени:\n{result}");
        }

        public void EnableNTP()
        {
            if (!CheckConnection()) return;
            var result = _timeService.EnableNTP();
            _logManager.AppendLog(result);
        }

        public void DisableNTP()
        {
            if (!CheckConnection()) return;
            var result = _timeService.DisableNTP();
            _logManager.AppendLog(result);
        }

        public void SetManualDateTime(DateTime date, DateTime time)
        {
            if (!CheckConnection()) return;
            var dateTime = $"{date:yyyy-MM-dd} {time:HH:mm:ss}";
            var result = _timeService.SetManualDateTime(dateTime);
            _logManager.AppendLog(result);
        }
    }
}