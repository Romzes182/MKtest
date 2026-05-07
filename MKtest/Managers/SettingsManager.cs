using MKtest.Configs;
using MKtest.Services;
using System;
using System.Windows.Forms;

namespace MKtest.Managers
{
    public class SettingsManager
    {
        // SSH Beelink
        private readonly TextBox _ipTextBox;
        private readonly NumericUpDown _portNumeric;
        private readonly TextBox _userTextBox;
        private readonly TextBox _passwordUserTextBox;
        private readonly TextBox _passwordRootTextBox;

        // Web Server
        private readonly TextBox _webServerIpTextBox;
        private readonly NumericUpDown _webServerPortNumeric;

        // USR Transfer
        private readonly TextBox _usrTransferIpTextBox;
        private readonly NumericUpDown _usrTransferPortNumeric;

        public SettingsManager(
            TextBox ipTextBox,
            NumericUpDown portNumeric,
            TextBox userTextBox,
            TextBox passwordUserTextBox,
            TextBox passwordRootTextBox,
            TextBox webServerIpTextBox = null,
            NumericUpDown webServerPortNumeric = null,
            TextBox usrTransferIpTextBox = null,
            NumericUpDown usrTransferPortNumeric = null)
        {
            _ipTextBox = ipTextBox;
            _portNumeric = portNumeric;
            _userTextBox = userTextBox;
            _passwordUserTextBox = passwordUserTextBox;
            _passwordRootTextBox = passwordRootTextBox;

            _webServerIpTextBox = webServerIpTextBox;
            _webServerPortNumeric = webServerPortNumeric;

            _usrTransferIpTextBox = usrTransferIpTextBox;
            _usrTransferPortNumeric = usrTransferPortNumeric;
        }

        public void LoadSettings()
        {
            // SSH Beelink
            var sshConfig = ConfigService.Config.SSHBeelink;
            _ipTextBox.Text = sshConfig.IP;
            _portNumeric.Value = sshConfig.Port;
            _userTextBox.Text = sshConfig.User;
            _passwordUserTextBox.Text = sshConfig.PasswordUser;
            _passwordRootTextBox.Text = sshConfig.PasswordRoot;

            // Web Server
            if (_webServerIpTextBox != null && _webServerPortNumeric != null)
            {
                var webConfig = ConfigService.Config.WebServer;
                _webServerIpTextBox.Text = webConfig.IpAddress;
                _webServerPortNumeric.Value = webConfig.Port;
            }

            // USR Transfer
            if (_usrTransferIpTextBox != null && _usrTransferPortNumeric != null)
            {
                var usrConfig = ConfigService.Config.USRTransfer;
                _usrTransferIpTextBox.Text = usrConfig.IP;
                _usrTransferPortNumeric.Value = usrConfig.Port;
            }
        }

        public bool SaveSettings(LogManager logManager)
        {
            try
            {
                // SSH Beelink
                ConfigService.Config.SSHBeelink.IP = _ipTextBox.Text;
                ConfigService.Config.SSHBeelink.Port = (int)_portNumeric.Value;
                ConfigService.Config.SSHBeelink.User = _userTextBox.Text;
                ConfigService.Config.SSHBeelink.PasswordUser = _passwordUserTextBox.Text;
                ConfigService.Config.SSHBeelink.PasswordRoot = _passwordRootTextBox.Text;

                // Web Server
                if (_webServerIpTextBox != null && _webServerPortNumeric != null)
                {
                    ConfigService.Config.WebServer.IpAddress = _webServerIpTextBox.Text;
                    ConfigService.Config.WebServer.Port = (int)_webServerPortNumeric.Value;
                }

                // USR Transfer
                if (_usrTransferIpTextBox != null && _usrTransferPortNumeric != null)
                {
                    ConfigService.Config.USRTransfer.IP = _usrTransferIpTextBox.Text;
                    ConfigService.Config.USRTransfer.Port = (int)_usrTransferPortNumeric.Value;
                }

                ConfigService.Save();
                logManager.AppendLog("Все настройки сохранены");
                return true;
            }
            catch (Exception ex)
            {
                logManager.AppendLog($"Ошибка сохранения настроек: {ex.Message}");
                return false;
            }
        }

        public bool SaveSshSettings(LogManager logManager)
        {
            try
            {
                ConfigService.Config.SSHBeelink.IP = _ipTextBox.Text;
                ConfigService.Config.SSHBeelink.Port = (int)_portNumeric.Value;
                ConfigService.Config.SSHBeelink.User = _userTextBox.Text;
                ConfigService.Config.SSHBeelink.PasswordUser = _passwordUserTextBox.Text;
                ConfigService.Config.SSHBeelink.PasswordRoot = _passwordRootTextBox.Text;

                ConfigService.Save();
                logManager.AppendLog("Настройки SSH Beelink сохранены");
                return true;
            }
            catch (Exception ex)
            {
                logManager.AppendLog($"Ошибка сохранения настроек SSH: {ex.Message}");
                return false;
            }
        }

        public bool SaveWebServerSettings(LogManager logManager)
        {
            try
            {
                if (_webServerIpTextBox != null && _webServerPortNumeric != null)
                {
                    ConfigService.Config.WebServer.IpAddress = _webServerIpTextBox.Text;
                    ConfigService.Config.WebServer.Port = (int)_webServerPortNumeric.Value;
                    ConfigService.Save();
                    logManager.AppendLog("Настройки веб-сервера сохранены");
                    return true;
                }
                logManager.AppendLog("Элементы управления веб-сервера не найдены");
                return false;
            }
            catch (Exception ex)
            {
                logManager.AppendLog($"Ошибка сохранения настроек веб-сервера: {ex.Message}");
                return false;
            }
        }

        public bool SaveUSRTransferSettings(LogManager logManager)
        {
            try
            {
                if (_usrTransferIpTextBox != null && _usrTransferPortNumeric != null)
                {
                    ConfigService.Config.USRTransfer.IP = _usrTransferIpTextBox.Text;
                    ConfigService.Config.USRTransfer.Port = (int)_usrTransferPortNumeric.Value;
                    ConfigService.Save();
                    logManager.AppendLog("Настройки USR Transfer сохранены");
                    return true;
                }
                logManager.AppendLog("Элементы управления USR Transfer не найдены");
                return false;
            }
            catch (Exception ex)
            {
                logManager.AppendLog($"Ошибка сохранения настроек USR Transfer: {ex.Message}");
                return false;
            }
        }

        public bool ResetAllSettings(LogManager logManager)
        {
            try
            {
                ConfigService.Config.SSHBeelink = new SSHConfig();
                ConfigService.Config.WebServer = new WebServerConfig();
                ConfigService.Config.USRTransfer = new USRTransferConfig();
                ConfigService.Save();
                LoadSettings();
                logManager.AppendLog("Все настройки сброшены");
                return true;
            }
            catch (Exception ex)
            {
                logManager.AppendLog($"Ошибка сброса настроек: {ex.Message}");
                return false;
            }
        }

        public bool ResetSshSettings(LogManager logManager)
        {
            try
            {
                ConfigService.Config.SSHBeelink = new SSHConfig();
                ConfigService.Save();
                LoadSettings();
                logManager.AppendLog("Настройки SSH Beelink сброшены");
                return true;
            }
            catch (Exception ex)
            {
                logManager.AppendLog($"Ошибка сброса настроек SSH: {ex.Message}");
                return false;
            }
        }

        public bool ResetWebServerSettings(LogManager logManager)
        {
            try
            {
                ConfigService.Config.WebServer = new WebServerConfig();
                ConfigService.Save();
                LoadSettings();
                logManager.AppendLog("Настройки веб-сервера сброшены");
                return true;
            }
            catch (Exception ex)
            {
                logManager.AppendLog($"Ошибка сброса настроек веб-сервера: {ex.Message}");
                return false;
            }
        }

        public bool ResetUSRTransferSettings(LogManager logManager)
        {
            try
            {
                ConfigService.Config.USRTransfer = new USRTransferConfig();
                ConfigService.Save();
                LoadSettings();
                logManager.AppendLog("Настройки USR Transfer сброшены");
                return true;
            }
            catch (Exception ex)
            {
                logManager.AppendLog($"Ошибка сброса настроек USR Transfer: {ex.Message}");
                return false;
            }
        }
    }
}