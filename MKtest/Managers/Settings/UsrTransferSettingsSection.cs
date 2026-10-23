using MKtest.Configs;
using System.Windows.Forms;

namespace MKtest.Managers.Settings
{
    /// <summary>Настройки подключения к ИР-0652.</summary>
    public sealed class UsrTransferSettingsSection : ISettingsSection
    {
        private readonly ISettingsStore _store;
        private readonly TextBox _ip;
        private readonly NumericUpDown _port;

        public UsrTransferSettingsSection(ISettingsStore store, TextBox ip, NumericUpDown port)
        {
            _store = store;
            _ip = ip;
            _port = port;
        }

        public void Load()
        {
            var cfg = _store.Config.USRTransfer;
            _ip.Text = cfg.IP;
            _port.Value = cfg.Port;
        }

        public void Apply()
        {
            var cfg = _store.Config.USRTransfer;
            cfg.IP = _ip.Text;
            cfg.Port = (int)_port.Value;
        }

        public void Reset() => _store.Config.USRTransfer = new USRTransferConfig();
    }
}
