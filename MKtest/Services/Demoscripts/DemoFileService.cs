using MKtest.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MKtest.Services.Demoscripts
{
    public class DemoFileService : IDemoFileService
    {
        private readonly DemoConfig _config;

        public DemoFileService(DemoConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(Path.GetFullPath(_config.ScenariosPath, AppContext.BaseDirectory));
            Directory.CreateDirectory(Path.GetFullPath(_config.TemplatesPath, AppContext.BaseDirectory));
        }

        public string GetScenarioPath(string scenarioName)
        {
            // Маппинг имен сценариев на папки
            var folderName = GetScenarioFolderName(scenarioName);
            return Path.Combine(Path.GetFullPath(_config.ScenariosPath, AppContext.BaseDirectory), folderName);
        }

        public string GetTemplatePath(string targetFile) =>
            Path.Combine(Path.GetFullPath(_config.TemplatesPath, AppContext.BaseDirectory), targetFile);

        private string GetScenarioFolderName(string scenarioName)
        {
            return scenarioName.ToLowerInvariant() switch
            {
                "demo-1" => "Demo1",
                "demo-2" => "Demo2",
                "demo-3" => "Demo3",
                "demo-4" => "Demo4",
                "demo-5" => "Demo5",
                "demo-45" => "demo-45",
                _ => scenarioName
            };
        }

        public async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Source file not found: {sourcePath}", sourcePath);

            await Task.Run(() => File.Copy(sourcePath, destinationPath, true));
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public async Task<string> ReadFileAsync(string filePath)
        {
            if (!FileExists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}", filePath);

            return await File.ReadAllTextAsync(filePath);
        }

        public IEnumerable<string> GetAvailableScenarios()
        {
            return _config.AvailableScenarios;
        }
    }
}
