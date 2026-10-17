using MKtest.Configs;
using System.Windows.Forms;

namespace MKtest.Managers.Settings
{
    /// <summary>Только настройки SSH Beelink.</summary>
    public sealed class SshSettingsSection : ISettingsSection
    {
        private readonly ISettingsStore _store;
        private readonly TextBox _ip, _user, _passwordUser, _passwordRoot;
        private readonly NumericUpDown _port;

        public SshSettingsSection(
            ISettingsStore store, TextBox ip, NumericUpDown port,
            TextBox user, TextBox passwordUser, TextBox passwordRoot)
        {
            _store = store;
            _ip = ip;
            _port = port;
            _user = user;
            _passwordUser = passwordUser;
            _passwordRoot = passwordRoot;
        }

        public void Load()
        {
            var cfg = _store.Config.SSHBeelink;
            _ip.Text = cfg.IP;
            _port.Value = cfg.Port;
            _user.Text = cfg.User;
            _passwordUser.Text = cfg.PasswordUser;
            _passwordRoot.Text = cfg.PasswordRoot;
        }

        public void Apply()
        {
            var cfg = _store.Config.SSHBeelink;
            cfg.IP = _ip.Text;
            cfg.Port = (int)_port.Value;
            cfg.User = _user.Text;
            cfg.PasswordUser = _passwordUser.Text;
            cfg.PasswordRoot = _passwordRoot.Text;
        }

        public void Reset() => _store.Config.SSHBeelink = new SSHConfig();
    }
}
