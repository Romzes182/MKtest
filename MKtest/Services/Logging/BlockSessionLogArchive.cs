using System;
using System.IO;
using System.Linq;

namespace MKtest.Services.Logging
{
    /// <summary>
    /// Complete on-disk journal for one protocol block in the current run.
    /// Separate from the bounded WinForms text window and shared main log.
    /// </summary>
    public sealed class BlockSessionLogArchive
    {
        private readonly object _sync = new();
        public string CurrentFile { get; }

        public BlockSessionLogArchive(string blockName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(blockName);

            string directory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "logs", "blocks");
            Directory.CreateDirectory(directory);

            string safeName = new string(blockName
                .Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ||
                              char.IsControl(ch) ? '_' : ch)
                .ToArray()).Trim();
            if (string.IsNullOrWhiteSpace(safeName))
                safeName = "block";

            CurrentFile = Path.Combine(directory,
                $"{DateTime.Now:yyyyMMdd_HHmmss_fff}_{safeName}_{Guid.NewGuid():N}.txt");
            File.WriteAllText(CurrentFile, string.Empty);
        }

        public void AppendLine(string message)
        {
            lock (_sync)
            {
                try
                {
                    File.AppendAllText(CurrentFile,
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Ошибка записи журнала блока: {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"Нет доступа к журналу блока: {ex.Message}");
                }
            }
        }
    }
}
