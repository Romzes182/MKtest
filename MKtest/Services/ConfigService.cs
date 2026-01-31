using MKtest.Configs;
using System;
using System.IO;
using System.Text.Json;


namespace MKtest.Services
{
    public class ConfigService
    {
        private static readonly string ConfigFile = "config.json";
        private static AppConfig? _config; 

        public static AppConfig Config
        {
            get
            {
                if (_config == null) Load();
                return _config!; 
            }
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    var json = File.ReadAllText(ConfigFile);
                    _config = JsonSerializer.Deserialize<AppConfig>(json);

                   
                    if (_config == null)
                    {
                        _config = new AppConfig();
                    }
                }
                else
                {
                    _config = new AppConfig();
                }
            }
            catch (Exception)
            {
                // В случае ошибки создаем новый конфиг
                _config = new AppConfig();
            }
        }

        public static void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_config, options);
                File.WriteAllText(ConfigFile, json);
            }
            catch (Exception ex)
            {
                // Можно добавить логирование
                Console.WriteLine($"Ошибка сохранения конфигурации: {ex.Message}");
            }
        }
    }
}