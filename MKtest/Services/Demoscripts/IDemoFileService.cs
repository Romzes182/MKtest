using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MKtest.Services.Demoscripts
{
    public interface IDemoFileService
    {
        string GetScenarioPath(string scenarioName);
        // Default implementation preserves compatibility with existing file services.
        string GetTemplatePath(string targetFile) =>
            Path.Combine(AppContext.BaseDirectory, "JsonTemplates", targetFile);
        Task CopyFileAsync(string sourcePath, string destinationPath);
        bool FileExists(string filePath);
        Task<string> ReadFileAsync(string filePath);
        IEnumerable<string> GetAvailableScenarios();
    }
}
