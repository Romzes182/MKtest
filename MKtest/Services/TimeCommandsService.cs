using System;

namespace MKtest.Services
{
    public class TimeCommandsService
    {
        private readonly SSHService _sshService;

        public event Action<string>? OnCommandExecuted;

        public TimeCommandsService(SSHService sshService)
        {
            _sshService = sshService;
        }

        public string CheckTimeStatus()
        {
            OnCommandExecuted?.Invoke("Проверка статуса времени...");
            // Используем ExecuteCommand вместо ExecuteDirectCommand для интерактивных команд
            return _sshService.ExecuteCommand("timedatectl status");
        }

        public string EnableNTP()
        {
            OnCommandExecuted?.Invoke("Включение NTP синхронизации...");
            return _sshService.ExecuteCommand("timedatectl set-ntp true");
        }

        public string DisableNTP()
        {
            OnCommandExecuted?.Invoke("Отключение NTP синхронизации...");
            return _sshService.ExecuteCommand("timedatectl set-ntp false");
        }

        public string SetManualDateTime(string dateTime)
        {
            OnCommandExecuted?.Invoke($"Установка времени: {dateTime}");
            return _sshService.ExecuteCommand($"date -s \"{dateTime}\"");
        }

      
    }
}