using MKtest.Configs;
using System.Windows.Forms;

namespace MKtest.Managers.Settings
{
    /// <summary>Только адрес и порт встроенного веб-сервера.</summary>
    public sealed class WebServerSettingsSection : ISettingsSection
    {
        private readonly ISettingsStore _store;
        private readonly TextBox _ip;
        private readonly NumericUpDown _port;

        public WebServerSettingsSection(ISettingsStore store, TextBox ip, NumericUpDown port)
        {
            _store = store;
            _ip = ip;
            _port = port;
        }

        public void Load()
        {
            var cfg = _store.Config.WebServer;
            _ip.Text = cfg.IpAddress;
            _port.Value = cfg.Port;
        }

        public void Apply()
        {
            var cfg = _store.Config.WebServer;
            cfg.IpAddress = _ip.Text;
            cfg.Port = (int)_port.Value;
        }

        public void Reset() => _store.Config.WebServer = new WebServerConfig();
    }
}
