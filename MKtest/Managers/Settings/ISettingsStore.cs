using MKtest.Configs;
using MKtest.Services;

namespace MKtest.Managers.Settings
{
    /// <summary>Абстракция общего хранилища настроек приложения.</summary>
    public interface ISettingsStore
    {
        AppConfig Config { get; }
        void Save();
    }

    /// <summary>
    /// Адаптер существующего ConfigService: формат config.json не меняется.
    /// </summary>
    public sealed class ConfigServiceSettingsStore : ISettingsStore
    {
        public AppConfig Config => ConfigService.Config;
        public void Save() => ConfigService.Save();
    }
}
