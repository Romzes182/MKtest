using MKtest.Services.SSH;
using System;

namespace MKtest.Services
{
    public sealed class TimeCommandsService
    {
        private readonly ISshCommandExecutor _commands;

        public event Action<string>? OnCommandExecuted;

        public TimeCommandsService(ISshCommandExecutor commands)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public string CheckTimeStatus()
        {
            OnCommandExecuted?.Invoke("Проверка статуса времени...");
            return _commands.ExecuteCommand("timedatectl status");
        }

        /// <summary>
        /// Actual local system time from the Beelink, not the operator's PC.
        /// ISO-style separator makes the command echo distinguishable from data.
        /// </summary>
        public string ReadCurrentTime() =>
            _commands.ExecuteCommand("date '+%Y-%m-%dT%H:%M:%S'");

        public string EnableNTP()
        {
            OnCommandExecuted?.Invoke("Включение NTP синхронизации...");
            return _commands.ExecuteCommand("timedatectl set-ntp true");
        }

        public string DisableNTP()
        {
            OnCommandExecuted?.Invoke("Отключение NTP синхронизации...");
            return _commands.ExecuteCommand("timedatectl set-ntp false");
        }

        public string SetManualDateTime(string dateTime)
        {
            OnCommandExecuted?.Invoke($"Установка времени: {dateTime}");
            return _commands.ExecuteCommand($"date -s \"{dateTime}\"");
        }
    }
}
