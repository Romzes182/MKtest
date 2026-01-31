using System;
using System.Collections.Generic;
using System.IO;

namespace MKtest.Services
{
    public class LoggerService
    {
        private readonly List<string> _logMessages = new();
        private readonly string _logFile;

        public event Action<string>? OnLogMessage;

        public LoggerService()
        {
            _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"log_{DateTime.Now:yyyyMMdd}.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(_logFile)!);
        }

        public void Log(string message)
        {
            string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";

            // Сохраняем в память
            _logMessages.Add(formattedMessage);

            // Записываем в файл
            try
            {
                File.AppendAllText(_logFile, formattedMessage + Environment.NewLine);
            }
            catch { }

            // Вызываем событие
            OnLogMessage?.Invoke(formattedMessage);

            // Выводим в консоль
            Console.WriteLine(formattedMessage);
        }

        public IReadOnlyList<string> GetLogs() => _logMessages.AsReadOnly();
    }
}