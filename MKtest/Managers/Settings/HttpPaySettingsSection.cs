using MKtest.Configs;
using System.Windows.Forms;

namespace MKtest.Managers.Settings
{
    /// <summary>Только параметры протокола HTTP-платежей.</summary>
    public sealed class HttpPaySettingsSection : ISettingsSection
    {
        private readonly ISettingsStore _store;
        private readonly TextBox _ip, _terminal, _route;
        private readonly NumericUpDown _port, _trip, _currentPayments, _interval;
        private readonly DateTimePicker _tripDate;

        public HttpPaySettingsSection(
            ISettingsStore store, TextBox ip, NumericUpDown port,
            TextBox terminal, TextBox route, NumericUpDown trip,
            DateTimePicker tripDate, NumericUpDown currentPayments, NumericUpDown interval)
        {
            _store = store;
            _ip = ip;
            _port = port;
            _terminal = terminal;
            _route = route;
            _trip = trip;
            _tripDate = tripDate;
            _currentPayments = currentPayments;
            _interval = interval;
        }

        public void Load()
        {
            var cfg = _store.Config.HTTPpay;
            _ip.Text = cfg.IP;
            _port.Value = cfg.Port;
            _terminal.Text = cfg.Terminal;
            _route.Text = cfg.Route;
            _trip.Value = cfg.Trip;
            _tripDate.Value = cfg.TripDate;
            _currentPayments.Value = cfg.CurrentPayments;
            _interval.Value = cfg.IntervalSeconds;
        }

        public void Apply()
        {
            var cfg = _store.Config.HTTPpay;
            cfg.IP = _ip.Text;
            cfg.Port = (int)_port.Value;
            cfg.Terminal = _terminal.Text;
            cfg.Route = _route.Text;
            cfg.Trip = (int)_trip.Value;
            cfg.TripDate = _tripDate.Value;
            cfg.CurrentPayments = (int)_currentPayments.Value;
            cfg.IntervalSeconds = (int)_interval.Value;
        }

        public void Reset() => _store.Config.HTTPpay = new HTTPpayConfig();
    }
}
