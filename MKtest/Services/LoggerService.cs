using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MKtest.Services
{
    public class LoggerService
    {
        private readonly List<string> _logMessages = new();
        private readonly string _logDirectory;
        private readonly string _logFile;
        private readonly object _lock = new object();

        public event Action<string>? OnLogMessage;

        public LoggerService()
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(_logDirectory);
            _logFile = Path.Combine(_logDirectory, $"log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        }

        public void Log(string message)
        {
            string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            lock (_lock)
            {
                // Сохраняем в память
                _logMessages.Add(formattedMessage);

                // Записываем в файл
                try
                {
                    File.AppendAllText(_logFile, formattedMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка записи в лог-файл: {ex.Message}");
                }

                // Ограничиваем размер лога в памяти (например, последние 1000 сообщений)
                if (_logMessages.Count > 1000)
                {
                    _logMessages.RemoveAt(0);
                }
            }

            // Вызываем событие
            OnLogMessage?.Invoke(formattedMessage);

            // Выводим в консоль
            Console.WriteLine(formattedMessage);
        }

        public IReadOnlyList<string> GetLogs() => _logMessages.AsReadOnly();

        public string GetLogFile() => _logFile;

        // Метод для получения всех лог-файлов
        public List<string> GetAllLogFiles()
        {
            try
            {
                return Directory.GetFiles(_logDirectory, "log_*.txt")
                    .OrderByDescending(f => f)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        // Метод для очистки старых логов (например, старше 7 дней)
        public void CleanOldLogs(int daysToKeep = 7)
        {
            try
            {
                var files = Directory.GetFiles(_logDirectory, "log_*.txt");
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < DateTime.Now.AddDays(-daysToKeep))
                    {
                        fileInfo.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка очистки старых логов: {ex.Message}");
            }
        }
    }
}