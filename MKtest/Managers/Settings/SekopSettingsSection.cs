using MKtest.Configs;
using System.Windows.Forms;

namespace MKtest.Managers.Settings
{
    /// <summary>Только адрес и порт протокола СЭКОП.</summary>
    public sealed class SekopSettingsSection : ISettingsSection
    {
        private readonly ISettingsStore _store;
        private readonly TextBox _ip;
        private readonly NumericUpDown _port;

        public SekopSettingsSection(ISettingsStore store, TextBox ip, NumericUpDown port)
        {
            _store = store;
            _ip = ip;
            _port = port;
        }

        public void Load()
        {
            var cfg = _store.Config.SekopProtocol;
            _ip.Text = cfg.IpAddress;
            _port.Value = cfg.Port;
        }

        public void Apply()
        {
            var cfg = _store.Config.SekopProtocol;
            cfg.IpAddress = _ip.Text;
            cfg.Port = (int)_port.Value;
        }

        public void Reset() => _store.Config.SekopProtocol = new SekopProtocolConfig();
    }
}
