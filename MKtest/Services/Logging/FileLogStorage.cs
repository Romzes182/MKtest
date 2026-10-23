using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MKtest.Services.Logging
{
    /// <summary>
    /// Отдельная ответственность: запись и обслуживание файлов logs/log_*.txt.
    /// Имя файла и формат строк соответствуют предыдущему LoggerService.
    /// </summary>
    public sealed class FileLogStorage : ILogFileStorage
    {
        private readonly object _sync = new();
        private readonly string _directory;
        private readonly string _file;

        public FileLogStorage(string? directory = null)
        {
            _directory = directory ??
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(_directory);
            _file = Path.Combine(_directory, $"log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        }

        public string DirectoryPath => _directory;
        public string CurrentFile => _file;

        public void AppendLine(string line)
        {
            lock (_sync)
            {
                try
                {
                    File.AppendAllText(_file, line + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка записи в лог-файл: {ex.Message}");
                }
            }
        }

        public List<string> GetAllLogFiles()
        {
            try
            {
                return Directory.GetFiles(_directory, "log_*.txt")
                    .OrderByDescending(path => path)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public void CleanOldLogs(int daysToKeep)
        {
            try
            {
                foreach (var file in Directory.GetFiles(_directory, "log_*.txt"))
                {
                    var info = new FileInfo(file);
                    if (info.CreationTime < DateTime.Now.AddDays(-daysToKeep))
                        info.Delete();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка очистки старых логов: {ex.Message}");
            }
        }
    }
}
