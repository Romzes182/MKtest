using MKtest.Managers;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKtest.UI.Sekop
{
    /// <summary>Существующие контролы формы, предоставленные конструктором.</summary>
    public sealed record SekopControls(
        NumericUpDown Transactions,
        NumericUpDown Passengers,
        Button Start,
        Button Stop,
        Label Status);

    /// <summary>
    /// Отвечает только за взаимодействие пользователя с СЭКОП.
    /// Кнопки Start/Stop уже подключены к MainForm через Designer,
    /// изменение NumericUpDown контроллер подписывает самостоятельно.
    /// </summary>
    public sealed class SekopUiController : IDisposable
    {
        private readonly ISekopProtocolOperations _operations;
        private readonly SekopControls _view;
        private readonly Action<string> _log;
        private bool _disposed;

        public SekopUiController(
            ISekopProtocolOperations operations,
            SekopControls view,
            Action<string> log)
        {
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _view.Transactions.Value = 0;
            _view.Passengers.Value = 0;
            SetStoppedState();

            _view.Transactions.ValueChanged += ValuesChanged;
            _view.Passengers.ValueChanged += ValuesChanged;
        }

        private SekopValues ReadValues()
        {
            if (_view.Transactions.InvokeRequired || _view.Passengers.InvokeRequired)
                return (SekopValues)_view.Transactions.Invoke(new Func<SekopValues>(ReadValues));

            return new SekopValues(
                (int)_view.Transactions.Value,
                (int)_view.Passengers.Value);
        }

        public async Task StartAsync()
        {
            if (_disposed || _operations.IsRunning)
                return;

            _view.Start.Enabled = false;
            _view.Stop.Enabled = true;

            try
            {
                await _operations.StartAsync(ReadValues);

                // Stop may have been pressed while the initial packet was sent.
                if (!_disposed && _operations.IsRunning)
                    _view.Status.Text = "Статус: Запущено";
                else if (!_disposed)
                    SetStoppedState();
            }
            catch (Exception ex)
            {
                if (_disposed)
                    return;

                if (!_operations.IsRunning && ex is OperationCanceledException)
                {
                    SetStoppedState();
                }
                else
                {
                    _view.Start.Enabled = true;
                    _view.Stop.Enabled = false;
                    _view.Status.Text = "Статус: Ошибка";
                    _log($"Ошибка запуска протокола СЭКОП: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            if (_disposed)
                return;

            _operations.Stop();
            SetStoppedState();
        }

        private async void ValuesChanged(object? sender, EventArgs e)
        {
            if (_disposed || !_operations.IsRunning)
                return;

            try
            {
                await _operations.SendCurrentAsync();
            }
            catch (OperationCanceledException) when (!_operations.IsRunning)
            {
                // Cancellation caused by Stop is not an error.
            }
            catch (Exception ex)
            {
                if (!_disposed)
                    _log($"Ошибка отправки СЭКОП при изменении значений: {ex.Message}");
            }
        }

        private void SetStoppedState()
        {
            _view.Start.Enabled = true;
            _view.Stop.Enabled = false;
            _view.Status.Text = "Статус: Остановлено";
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _view.Transactions.ValueChanged -= ValuesChanged;
            _view.Passengers.ValueChanged -= ValuesChanged;
        }
    }
}
