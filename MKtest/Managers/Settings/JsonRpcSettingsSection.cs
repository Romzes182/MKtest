using MKtest.Configs;
using System.Windows.Forms;

namespace MKtest.Managers.Settings
{
    /// <summary>Настройки адреса JSON-RPC (имя HTTPprotokol сохранено в AppConfig).</summary>
    public sealed class JsonRpcSettingsSection : ISettingsSection
    {
        private readonly ISettingsStore _store;
        private readonly TextBox _ip;
        private readonly NumericUpDown _port;

        public JsonRpcSettingsSection(ISettingsStore store, TextBox ip, NumericUpDown port)
        {
            _store = store;
            _ip = ip;
            _port = port;
        }

        public void Load()
        {
            var cfg = _store.Config.HTTPprotokol;
            _ip.Text = cfg.IP;
            _port.Value = cfg.Port;
        }

        public void Apply()
        {
            var cfg = _store.Config.HTTPprotokol;
            cfg.IP = _ip.Text;
            cfg.Port = (int)_port.Value;
        }

        public void Reset() => _store.Config.HTTPprotokol = new JSONRPCprotokolConfig();
    }
}
