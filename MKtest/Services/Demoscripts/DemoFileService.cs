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
            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _config.ScenariosPath)));
            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _config.TemplatesPath)));
        }

        public string GetScenarioPath(string scenarioName)
        {
            // Маппинг имен сценариев на папки
            var folderName = GetScenarioFolderName(scenarioName);
            return Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _config.ScenariosPath)), folderName);
        }

        public string GetTemplatePath(string targetFile) =>
            Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _config.TemplatesPath)), targetFile);

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

            return await FrameworkFiles.ReadAllTextAsync(filePath);
        }

        public IEnumerable<string> GetAvailableScenarios()
        {
            return _config.AvailableScenarios;
        }
    }
}
