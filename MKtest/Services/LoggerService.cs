using MKtest.Services.Logging;
using System;
using System.Collections.Generic;

namespace MKtest.Services
{
    /// <summary>
    /// Доставляет записи в независимое файловое хранилище и ограниченную историю.
    /// Не зависит от WinForms и сохраняет прежний публичный API.
    /// </summary>
    public sealed class LoggerService
    {
        private readonly ILogFileStorage _storage;
        private readonly BoundedLogHistory _history;
        private readonly object _sync = new();

        public event Action<string>? OnLogMessage;

        public LoggerService()
            : this(new FileLogStorage(), new BoundedLogHistory())
        {
        }

        public LoggerService(ILogFileStorage storage, BoundedLogHistory history)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _history = history ?? throw new ArgumentNullException(nameof(history));
        }

        public void Log(string message)
        {
            string formatted;
            lock (_sync)
            {
                formatted = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                _history.Add(formatted);
                _storage.AppendLine(formatted);
            }

            OnLogMessage?.Invoke(formatted);
            Console.WriteLine(formatted);
        }

        public IReadOnlyList<string> GetLogs() => _history.Snapshot();

        public string GetLogFile() => _storage.CurrentFile;

        public List<string> GetAllLogFiles() => _storage.GetAllLogFiles();

        public void CleanOldLogs(int daysToKeep = 7) =>
            _storage.CleanOldLogs(daysToKeep);
    }
}
