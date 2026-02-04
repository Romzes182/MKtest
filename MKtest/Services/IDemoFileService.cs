using System.Collections.Generic;
using System.Threading.Tasks;

namespace MKtest.Services
{
    public interface IDemoFileService
    {
        string GetScenarioPath(string scenarioName);
        Task CopyFileAsync(string sourcePath, string destinationPath);
        bool FileExists(string filePath);
        Task<string> ReadFileAsync(string filePath);
        IEnumerable<string> GetAvailableScenarios();
    }
}