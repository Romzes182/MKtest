using System.Collections.Generic;

namespace MKtest.Services.Logging
{
    /// <summary>Хранение журнала на диске независимо от UI.</summary>
    public interface ILogFileStorage
    {
        string CurrentFile { get; }
        string DirectoryPath { get; }
        void AppendLine(string line);
        List<string> GetAllLogFiles();
        void CleanOldLogs(int daysToKeep);
    }
}
