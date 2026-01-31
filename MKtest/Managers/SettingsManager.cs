using MKtest.Configs;
using MKtest.Services;
using System.Windows.Forms;

namespace MKtest.Managers
{
    public class SettingsManager
    {
        private readonly TextBox _ipTextBox;
        private readonly NumericUpDown _portNumeric;
        private readonly TextBox _userTextBox;
        private readonly TextBox _passwordUserTextBox;
        private readonly TextBox _passwordRootTextBox;

        public SettingsManager(
            TextBox ipTextBox,
            NumericUpDown portNumeric,
            TextBox userTextBox,
            TextBox passwordUserTextBox,
            TextBox passwordRootTextBox)
        {
            _ipTextBox = ipTextBox;
            _portNumeric = portNumeric;
            _userTextBox = userTextBox;
            _passwordUserTextBox = passwordUserTextBox;
            _passwordRootTextBox = passwordRootTextBox;
        }

        public void LoadSettings()
        {
            var config = ConfigService.Config.SSHBeelink;

            _ipTextBox.Text = config.IP;
            _portNumeric.Value = config.Port;
            _userTextBox.Text = config.User;
            _passwordUserTextBox.Text = config.PasswordUser;
            _passwordRootTextBox.Text = config.PasswordRoot;
        }

        public bool SaveSettings(LogManager logManager)
        {
            try
            {
                var config = ConfigService.Config.SSHBeelink;

                config.IP = _ipTextBox.Text;
                config.Port = (int)_portNumeric.Value;
                config.User = _userTextBox.Text;
                config.PasswordUser = _passwordUserTextBox.Text;
                config.PasswordRoot = _passwordRootTextBox.Text;

                ConfigService.Save();
                logManager.AppendLog("Настройки SSH сохранены");
                return true;
            }
            catch (System.Exception ex)
            {
                logManager.AppendLog($"Ошибка сохранения настроек: {ex.Message}");
                return false;
            }
        }

        public bool ResetSettings(LogManager logManager)
        {
            try
            {
                ConfigService.Config.SSHBeelink = new SSHConfig();
                ConfigService.Save();
                LoadSettings();
                logManager.AppendLog("Настройки SSH сброшены к значениям по умолчанию");
                return true;
            }
            catch (System.Exception ex)
            {
                logManager.AppendLog($"Ошибка сброса настроек: {ex.Message}");
                return false;
            }
        }
    }
}