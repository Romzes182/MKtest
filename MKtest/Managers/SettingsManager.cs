using MKtest.Managers.Settings;
using System;

namespace MKtest.Managers
{
    /// <summary>
    /// Координатор сохранения настроек. Каждая группа реализована отдельно,
    /// поэтому добавление протокола не увеличивает этот класс бизнес-логикой UI.
    /// </summary>
    public sealed class SettingsManager
    {
        private readonly ISettingsStore _store;
        private readonly ISettingsSection _ssh, _web, _usr, _jsonRpc, _httpPay, _sekop;
        private readonly HermesSettingsSection _hermes;

        public SettingsManager(
            ISettingsStore store,
            ISettingsSection ssh,
            ISettingsSection web,
            ISettingsSection usr,
            HermesSettingsSection hermes,
            ISettingsSection jsonRpc,
            ISettingsSection httpPay,
            ISettingsSection sekop)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _ssh = ssh ?? throw new ArgumentNullException(nameof(ssh));
            _web = web ?? throw new ArgumentNullException(nameof(web));
            _usr = usr ?? throw new ArgumentNullException(nameof(usr));
            _hermes = hermes ?? throw new ArgumentNullException(nameof(hermes));
            _jsonRpc = jsonRpc ?? throw new ArgumentNullException(nameof(jsonRpc));
            _httpPay = httpPay ?? throw new ArgumentNullException(nameof(httpPay));
            _sekop = sekop ?? throw new ArgumentNullException(nameof(sekop));
        }

        public void LoadSettings()
        {
            _ssh.Load();
            _web.Load();
            _usr.Load();
            _hermes.Load();
            _jsonRpc.Load();
            _httpPay.Load();
            _sekop.Load();
        }

        public bool SaveSettings(LogManager logManager) =>
            Run(logManager, () =>
            {
                // Совместимость: прежний SaveSettings не сохранял Гермес.
                _ssh.Apply();
                _web.Apply();
                _usr.Apply();
                _jsonRpc.Apply();
                _httpPay.Apply();
                _sekop.Apply();
                _store.Save();
            }, "Все настройки сохранены", "Ошибка сохранения настроек");

        public bool SaveSshSettings(LogManager logManager) =>
            SaveSection(_ssh, logManager, "Настройки SSH Beelink сохранены", "Ошибка сохранения настроек SSH");

        public bool SaveWebServerSettings(LogManager logManager) =>
            SaveSection(_web, logManager, "Настройки веб-сервера сохранены", "Ошибка сохранения настроек веб-сервера");

        public bool SaveUSRTransferSettings(LogManager logManager) =>
            SaveSection(_usr, logManager, "Настройки USR Transfer сохранены", "Ошибка сохранения настроек USR Transfer");

        public bool SaveHermesSettings(LogManager logManager) => _hermes.Save(logManager);

        public bool SaveHTTPpaySettings(LogManager logManager) =>
            SaveSection(_httpPay, logManager, "Настройки HTTPpay сохранены", "Ошибка HTTPpay");

        public bool SaveSekopProtocolSettings(LogManager logManager) =>
            SaveSection(_sekop, logManager, "Настройки протокола СЭКОП сохранены", "Ошибка сохранения настроек протокола СЭКОП");

        public void ResetHermesSettings(LogManager logManager) => _hermes.Reset(logManager);

        public bool ResetSekopProtocolSettings(LogManager logManager) =>
            ResetSection(_sekop, logManager, "Настройки протокола СЭКОП сброшены", "Ошибка сброса настроек протокола СЭКОП");

        public bool ResetAllSettings(LogManager logManager) =>
            Run(logManager, () =>
            {
                // Совместимость: прежняя команда сбрасывала только эти три группы.
                _ssh.Reset();
                _web.Reset();
                _usr.Reset();
                _store.Save();
                LoadSettings();
            }, "Все настройки сброшены", "Ошибка сброса настроек");

        public bool ResetSshSettings(LogManager logManager) =>
            ResetSection(_ssh, logManager, "Настройки SSH Beelink сброшены", "Ошибка сброса настроек SSH");

        public bool ResetWebServerSettings(LogManager logManager) =>
            ResetSection(_web, logManager, "Настройки веб-сервера сброшены", "Ошибка сброса настроек веб-сервера");

        public bool ResetUSRTransferSettings(LogManager logManager) =>
            ResetSection(_usr, logManager, "Настройки USR Transfer сброшены", "Ошибка сброса настроек USR Transfer");

        public bool ResetHTTPProtokolSettings(LogManager logManager) =>
            ResetSection(_jsonRpc, logManager, "Настройки JSONRPC протокола сброшены", "Ошибка сброса JSONRPC протокол");

        public bool ResetHTTPpaySettings(LogManager logManager) =>
            ResetSection(_httpPay, logManager, "Настройки HTTPpay сброшены", "Ошибка сброса HTTPpay");

        private bool SaveSection(ISettingsSection section, LogManager logManager, string success, string failure) =>
            Run(logManager, () =>
            {
                section.Apply();
                _store.Save();
            }, success, failure);

        private bool ResetSection(ISettingsSection section, LogManager logManager, string success, string failure) =>
            Run(logManager, () =>
            {
                section.Reset();
                _store.Save();
                LoadSettings();
            }, success, failure);

        private static bool Run(LogManager logManager, Action action, string success, string failure)
        {
            try
            {
                action();
                logManager.AppendLog(success);
                return true;
            }
            catch (Exception ex)
            {
                logManager.AppendLog($"{failure}: {ex.Message}");
                return false;
            }
        }
    }
}
