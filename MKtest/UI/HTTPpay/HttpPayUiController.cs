using MKtest.Managers;
using System;
using System.Windows.Forms;

namespace MKtest.UI.HTTPpay
{
    /// <summary>
    /// UI state and live pTotal for HTTP payments; network requests stay in services.
    /// </summary>
    public sealed class HttpPayUiController
    {
        private readonly IHttpPayOperations _operations;
        private readonly Func<int> _interval;
        private readonly NumericUpDown _total;
        private readonly Button _start, _stop;
        private readonly Label _status;
        private readonly Action<string> _log;

        public HttpPayUiController(
            IHttpPayOperations operations, Func<int> interval,
            NumericUpDown total, int defaultTotal,
            Button start, Button stop, Label status, Action<string> log)
        {
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            _interval = interval ?? throw new ArgumentNullException(nameof(interval));
            _total = total ?? throw new ArgumentNullException(nameof(total));
            _start = start ?? throw new ArgumentNullException(nameof(start));
            _stop = stop ?? throw new ArgumentNullException(nameof(stop));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _total.Value = Math.Clamp(defaultTotal, (int)_total.Minimum, (int)_total.Maximum);
            SetRunning(false);
        }

        public void Start()
        {
            if (_operations.IsRunning) return;

            try
            {
                int interval = _interval();
                if (interval <= 0)
                    throw new ArgumentOutOfRangeException(nameof(interval), "Интервал должен быть больше 0.");

                // Pass a function: new values are read before EVERY packet.
                _operations.Start(ReadTotal, interval);
                SetRunning(_operations.IsRunning);
                if (_operations.IsRunning)
                    _log($"HTTPpay запущен, интервал {interval} сек");
            }
            catch (Exception ex)
            {
                _log($"HTTPpay: {ex.Message}");
                SetRunning(false);
            }
        }

        public void Stop()
        {
            _operations.Stop();
            SetRunning(false);
        }

        private int ReadTotal()
        {
            if (_total.IsDisposed)
                throw new ObjectDisposedException(nameof(_total));

            if (_total.InvokeRequired)
                return (int)_total.Invoke(new Func<int>(() => (int)_total.Value));

            return (int)_total.Value;
        }

        private void SetRunning(bool running)
        {
            _start.Enabled = !running;
            _stop.Enabled = running;
            _status.Text = running ? "Статус: Запущено" : "Статус: Остановлено";
            _total.Enabled = true;
        }
    }
}
