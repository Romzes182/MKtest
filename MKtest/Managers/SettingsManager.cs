using MKtest.Configs;
using MKtest.Managers.Settings;
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

        private readonly HermesSettingsSection _hermesSection;

        private readonly TextBox _httpProtokolIpTextBox;
        private readonly NumericUpDown _httpProtokolPortNumeric;


        private readonly TextBox _httpPayIpTextBox;
        private readonly NumericUpDown _httpPayPortNumeric;
        private readonly TextBox _httpPayTerminalTextBox;
        private readonly TextBox _httpPayRouteTextBox;
        private readonly NumericUpDown _httpPayTripNumeric;
        private readonly DateTimePicker _httpPayTripDatePicker;
        private readonly NumericUpDown _httpPayCurrentNumeric;
        private readonly NumericUpDown _httpPayIntervalNumeric;
        public SettingsManager(TextBox ipTextBox, NumericUpDown portNumeric, TextBox userTextBox, TextBox passwordUserTextBox, TextBox passwordRootTextBox,
            TextBox webServerIpTextBox, NumericUpDown webServerPortNumeric, TextBox usrTransferIpTextBox, NumericUpDown usrTransferPortNumeric,
            TextBox hermesIpTextBox, NumericUpDown hermesPortNumeric, TextBox hermesUserTextBox, TextBox hermesPasswordTextBox, TextBox httpProtokolIpTextBox,
            NumericUpDown httpProtokolPortNumeric, TextBox httpPayIpTextBox, NumericUpDown httpPayPortNumeric, TextBox httpPayTerminalTextBox, TextBox httpPayRouteTextBox,
            NumericUpDown httpPayTripNumeric, DateTimePicker httpPayTripDatePicker, NumericUpDown httpPayCurrentNumeric, NumericUpDown httpPayIntervalNumeric)
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

            _hermesSection = new HermesSettingsSection(hermesIpTextBox, hermesPortNumeric, hermesUserTextBox, hermesPasswordTextBox );

            _httpProtokolIpTextBox = httpProtokolIpTextBox;
            _httpProtokolPortNumeric = httpProtokolPortNumeric;

            _httpPayIpTextBox = httpPayIpTextBox;
            _httpPayPortNumeric = httpPayPortNumeric;
            _httpPayTerminalTextBox = httpPayTerminalTextBox;
            _httpPayRouteTextBox = httpPayRouteTextBox;
            _httpPayTripNumeric = httpPayTripNumeric;
            _httpPayTripDatePicker = httpPayTripDatePicker;
            _httpPayCurrentNumeric = httpPayCurrentNumeric;
            _httpPayIntervalNumeric = httpPayIntervalNumeric;
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

            // Hermes SSH
            _hermesSection.Load();

            //httpprotokol

            if (_httpProtokolIpTextBox != null && _httpProtokolPortNumeric != null)
            {
                var httpConfig = ConfigService.Config.HTTPprotokol;
                _httpProtokolIpTextBox.Text = httpConfig.IP;
                _httpProtokolPortNumeric.Value = httpConfig.Port;
            }

            // HTTPpay
            if (_httpPayIpTextBox != null &&
                _httpPayPortNumeric != null &&
                _httpPayTerminalTextBox != null &&
                _httpPayRouteTextBox != null &&
                _httpPayTripNumeric != null &&
                _httpPayTripDatePicker != null &&
                _httpPayCurrentNumeric != null &&
                _httpPayIntervalNumeric != null)
            {
                var cfg = ConfigService.Config.HTTPpay;

                _httpPayIpTextBox.Text = cfg.IP;
                _httpPayPortNumeric.Value = cfg.Port;
                _httpPayTerminalTextBox.Text = cfg.Terminal;
                _httpPayRouteTextBox.Text = cfg.Route;
                _httpPayTripNumeric.Value = cfg.Trip;
                _httpPayTripDatePicker.Value = cfg.TripDate;
                _httpPayCurrentNumeric.Value = cfg.CurrentPayments;
                _httpPayIntervalNumeric.Value = cfg.IntervalSeconds;
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

                if (_httpProtokolIpTextBox != null && _httpProtokolPortNumeric != null)
                {
                    ConfigService.Config.HTTPprotokol.IP = _httpProtokolIpTextBox.Text;
                    ConfigService.Config.HTTPprotokol.Port = (int)_httpProtokolPortNumeric.Value;
                }

                // HTTPpay
                if (_httpPayIpTextBox != null &&
                    _httpPayPortNumeric != null &&
                    _httpPayTerminalTextBox != null &&
                    _httpPayRouteTextBox != null &&
                    _httpPayTripNumeric != null &&
                    _httpPayTripDatePicker != null &&
                    _httpPayCurrentNumeric != null &&
                    _httpPayIntervalNumeric != null)
                {
                    var cfg = ConfigService.Config.HTTPpay;
                    cfg.IP = _httpPayIpTextBox.Text;
                    cfg.Port = (int)_httpPayPortNumeric.Value;
                    cfg.Terminal = _httpPayTerminalTextBox.Text;
                    cfg.Route = _httpPayRouteTextBox.Text;
                    cfg.Trip = (int)_httpPayTripNumeric.Value;
                    cfg.TripDate = _httpPayTripDatePicker.Value;
                    cfg.CurrentPayments = (int)_httpPayCurrentNumeric.Value;
                    cfg.IntervalSeconds = (int)_httpPayIntervalNumeric.Value;
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

        public bool SaveHermesSettings(LogManager log)
        {
            return _hermesSection.Save(log);
        }

        public bool SaveHTTPpaySettings(LogManager logManager)
        {
            try
            {
                var cfg = ConfigService.Config.HTTPpay;
                cfg.IP = _httpPayIpTextBox.Text;
                cfg.Port = (int)_httpPayPortNumeric.Value;
                cfg.Terminal = _httpPayTerminalTextBox.Text;
                cfg.Route = _httpPayRouteTextBox.Text;
                cfg.Trip = (int)_httpPayTripNumeric.Value;
                cfg.TripDate = _httpPayTripDatePicker.Value;
                cfg.CurrentPayments = (int)_httpPayCurrentNumeric.Value;
                cfg.IntervalSeconds = (int)_httpPayIntervalNumeric.Value;
                ConfigService.Save();
                logManager.AppendLog("Настройки HTTPpay сохранены");
                return true;
            }
            catch (Exception ex)
            {
                logManager.AppendLog($"Ошибка HTTPpay: {ex.Message}");
                return false;
            }
        }
        public void ResetHermesSettings(LogManager log)
        {
            _hermesSection.Reset(log);
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

        public bool ResetHTTPProtokolSettings(LogManager logManager)
        {
            try
            {
                ConfigService.Config.HTTPprotokol = new JSONRPCprotokolConfig();
                ConfigService.Save();
                LoadSettings();
                logManager.AppendLog("Настройки JSONRPC протокола сброшены");
                return true;
            }
            catch (Exception ex)
            {
                logManager.AppendLog($"Ошибка сброса JSONRPC протокол: {ex.Message}");
                return false;
            }
        }
        public bool ResetHTTPpaySettings(LogManager logManager)
        {
            try
            {
                ConfigService.Config.HTTPpay = new HTTPpayConfig();

                ConfigService.Save();

                LoadSettings();

                logManager.AppendLog("Настройки HTTPpay сброшены");

                return true;
            }
            catch (Exception ex)
            {
                logManager.AppendLog($"Ошибка сброса HTTPpay: {ex.Message}");
                return false;
            }
        }
    }
}