using MKtest.Managers;
using MKtest.Services.JSONRPCprotokol;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKtest.UI.JSONRPC
{
    /// <summary>All JSON-RPC button state, inputs and test actions.</summary>
    public sealed class JsonRpcUiController
    {
        private readonly IJsonRpcOperations _operations;
        private readonly IJsonRpcSender _sender;
        private readonly TextBox _counter, _interval;
        private readonly Button _start, _stop;
        private readonly Label _status;
        private readonly Action<string> _log;

        public JsonRpcUiController(
            IJsonRpcOperations operations, IJsonRpcSender sender,
            TextBox counter, TextBox interval,
            Button start, Button stop, Label status, Action<string> log)
        {
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _counter = counter ?? throw new ArgumentNullException(nameof(counter));
            _interval = interval ?? throw new ArgumentNullException(nameof(interval));
            _start = start ?? throw new ArgumentNullException(nameof(start));
            _stop = stop ?? throw new ArgumentNullException(nameof(stop));
            _status = status ?? throw new ArgumentNullException(nameof(status));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _counter.Text = "0";
            _interval.Text = "5";
            UpdateRunning(false);
        }

        public void Start()
        {
            if (_operations.IsRunning) return;
            try
            {
                var (counter, interval) = ReadInputs();
                _operations.Start(counter, interval);
                UpdateRunning(_operations.IsRunning);
                if (_operations.IsRunning)
                    _log("JSONRPC запущен");
            }
            catch (Exception ex) { _log($"JSONRPC: {ex.Message}"); }
        }

        public void Stop()
        {
            _operations.Stop();
            UpdateRunning(false);
            _log("JsonRPCprotokol остановлен");
        }

        public async Task TestAsync()
        {
            try
            {
                int counter = int.Parse(_counter.Text);
                await _sender.SendAsync(counter, CancellationToken.None);
                _log("JSONRPC тест отправлен");
            }
            catch (Exception ex) { _log($"JSONRPC: {ex.Message}"); }
        }

        public void Update()
        {
            try
            {
                var (counter, interval) = ReadInputs();
                _operations.UpdateValues(counter, interval);
                _log("JSONRPCprotokol параметры обновлены");
            }
            catch (Exception ex) { _log($"JSONRPC: {ex.Message}"); }
        }

        private (int Counter, int Interval) ReadInputs()
        {
            if (!int.TryParse(_counter.Text, out int counter))
                throw new FormatException("Некорректное количество транзакций");
            if (!int.TryParse(_interval.Text, out int interval) || interval <= 0)
                throw new FormatException("Интервал должен быть положительным числом секунд");
            return (counter, interval);
        }

        private void UpdateRunning(bool running)
        {
            _start.Enabled = !running;
            _stop.Enabled = running;
            _status.Text = running ? "Статус: Запущено" : "Статус: Остановлено";
        }
    }
}
