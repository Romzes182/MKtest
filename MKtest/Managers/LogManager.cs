using MKtest.Services;
using MKtest.Services.Logging;
using MKtest.UI.Logging;
using System;
using System.IO;
using System.Windows.Forms;

namespace MKtest.Managers
{
    /// <summary>
    /// Делегирует запись на диск и отрисовку двум независимым компонентам.
    /// Общий интерфейс для существующих менеджеров MKtest не меняется.
    /// </summary>
    public sealed class LogManager : IDisposable
    {
        private readonly LoggerService _loggerService;
        private readonly IUiLogSink _uiSink;
        private readonly BlockSessionLogArchive? _blockArchive;

        public LoggerService LoggerService => _loggerService;

        public LogManager(TextBox logTextBox)
            : this(new LoggerService(), new WinFormsLogSink(logTextBox))
        {
        }

        public LogManager(LoggerService loggerService, IUiLogSink uiSink)
            : this(loggerService, uiSink, null)
        {
        }

        private LogManager(
            LoggerService loggerService, IUiLogSink uiSink,
            BlockSessionLogArchive? blockArchive)
        {
            _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
            _uiSink = uiSink ?? throw new ArgumentNullException(nameof(uiSink));
            _blockArchive = blockArchive;
        }

        /// <summary>
        /// The returned logger saves to the same file and displays to both
        /// the general log and the selected block; never to other blocks.
        /// </summary>
        public LogManager CreateScope(TextBox localTextBox) =>
            CreateScope(localTextBox, "Раздел");

        public LogManager CreateScope(TextBox localTextBox, string blockName) =>
            new(_loggerService, new ScopedLogSink(
                _uiSink, new WinFormsLogSink(localTextBox)),
                new BlockSessionLogArchive(blockName));

        /// <summary>
        /// Full current session file: block-specific for a scoped logger,
        /// general session file for the shared logger. Never a UI snapshot.
        /// </summary>
        public string GetCurrentSessionLogFile() =>
            _blockArchive?.CurrentFile ?? _loggerService.GetLogFile();

        /// <summary>
        /// Enables following the newest entry after the user has scrolled up.
        /// Never changes what is stored in the journal file.
        /// </summary>
        public void ResumeLogScroll()
        {
            if (_uiSink is ILogViewportControl viewport)
                viewport.ResumeToLatest();
        }

        public void AppendLog(string message)
        {
            // File and in-memory history are written immediately.
            _loggerService.Log(message);
            _blockArchive?.AppendLine(message);

            // Only screen refresh is batched (no blocking Control.Invoke).
            _uiSink.Append($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        public void ClearLog()
        {
            // Preserve the original marker in the log file.
            _loggerService.Log("--- Лог очищен ---");
            _blockArchive?.AppendLine("--- Лог очищен ---");
            _uiSink.Clear();
        }

        public string GetLogFilePath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        public void Dispose() => _uiSink.Dispose();
    }
}
