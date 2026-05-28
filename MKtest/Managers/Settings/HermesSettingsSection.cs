using MKtest.Configs;
using MKtest.Services;
using System;
using System.Windows.Forms;

namespace MKtest.Managers.Settings
{
    public class HermesSettingsSection
    {
        private readonly TextBox _ip;
        private readonly NumericUpDown _port;
        private readonly TextBox _user;
        private readonly TextBox _password;

        public HermesSettingsSection(
            TextBox ip,
            NumericUpDown port,
            TextBox user,
            TextBox password)
        {
            _ip = ip;
            _port = port;
            _user = user;
            _password = password;
        }

        public void Load()
        {
            var cfg = ConfigService.Config.HermesSSH;
            _ip.Text = cfg.IP;
            _port.Value = Clamp(cfg.Port, _port);
            _user.Text = cfg.User;
            _password.Text = cfg.Password;
        }

        public bool Save(LogManager log)
        {
            try
            {
                var cfg = ConfigService.Config.HermesSSH;
                cfg.IP = _ip.Text.Trim();
                cfg.Port = (int)_port.Value;
                cfg.User = _user.Text.Trim();
                cfg.Password = _password.Text;
                ConfigService.Save();
                log.AppendLog("Настройки Hermes SSH сохранены");
                return true;
            }
            catch (Exception ex)
            {
                log.AppendLog($"Ошибка сохранения Hermes SSH: {ex.Message}");
                return false;
            }
        }

        public void Reset(LogManager log)
        {
            ConfigService.Config.HermesSSH = new HermesSSHConfig();
            Load();
            ConfigService.Save();
            log.AppendLog("Настройки Hermes SSH сброшены");
        }

        private static decimal Clamp(int value, NumericUpDown num)
        {
            if (value < num.Minimum) return num.Minimum;
            if (value > num.Maximum) return num.Maximum;
            return value;
        }
    }
}