using MKtest.Managers;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKtest.UI.Hermes
{
    /// <summary>Existing Hermes controls; Designer continues to own the controls.</summary>
    public sealed record HermesControls(
        TextBox Entered,
        TextBox Exited,
        Button Start,
        Button Stop,
        Button Test,
        Button Update,
        Label Status);

    /// <summary>
    /// Only the Hermes WinForms behavior. SSH connection, periods and
    /// protocol data remain the responsibility of IHermesPassengerOperations.
    /// </summary>
    public sealed class HermesUiController : IDisposable
    {
        private readonly IHermesPassengerOperations _operations;
        private readonly HermesControls _view;
        private readonly Action<string> _log;
        private bool _disposed;

        public HermesUiController(
            IHermesPassengerOperations operations,
            HermesControls view,
            Action<string> log)
        {
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _view.Entered.Text = "0";
            _view.Exited.Text = "0";
            _view.Status.Text = "Статус: Остановлено";
            SetButtons(false);

            _operations.StatusChanged += OperationsStatusChanged;
        }

        public async Task StartAsync()
        {
            if (_disposed || _operations.IsRunning)
                return;
            if (!TryReadValues(out int entered, out int exited))
                return;

            SetButtons(true);
            _view.Status.Text = "Статус: Запуск...";

            try
            {
                await _operations.StartAsync(entered, exited);
            }
            catch (Exception ex)
            {
                if (_disposed) return;
                _log($"Hermes start error: {ex.Message}");
                _view.Status.Text = "Статус: Ошибка подключения";
                MessageBox.Show($"Hermes недоступен:\n{ex.Message}", "Hermes",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (!_disposed && !_view.Status.IsDisposed && !_operations.IsRunning)
                    SetButtons(false);
            }
        }

        public async Task StopAsync()
        {
            if (_disposed) return;
            try
            {
                await _operations.StopAsync();
            }
            catch (Exception ex)
            {
                if (!_disposed)
                    _log($"Hermes stop error: {ex.Message}");
            }
            finally
            {
                if (!_disposed && !_view.Status.IsDisposed)
                {
                    SetButtons(false);
                    _view.Status.Text = "Статус: Остановлено";
                }
            }
        }

        public async Task TestAsync()
        {
            if (_disposed) return;
            try
            {
                _view.Status.Text = "Статус: Тест...";
                var ok = await _operations.TestConnectionAsync();
                if (_disposed || _view.Status.IsDisposed) return;

                _view.Status.Text = ok
                    ? "Статус: Доступно"
                    : "Статус: Недоступно";
                _log(ok
                    ? "Hermes SSH: подключение успешно"
                    : "Hermes SSH: подключение не удалось");
            }
            catch (Exception ex)
            {
                if (_disposed || _view.Status.IsDisposed) return;
                _log($"Hermes test fatal: {ex.Message}");
                _view.Status.Text = "Статус: Ошибка";
            }
        }

        public void Update()
        {
            if (_disposed || !TryReadValues(out int entered, out int exited))
                return;

            try
            {
                _operations.UpdateValues(entered, exited);
                _log($"Hermes: обновлены значения A={entered}, B={exited}");
            }
            catch (Exception ex)
            {
                _log($"Hermes update error: {ex.Message}");
            }
        }

        private bool TryReadValues(out int entered, out int exited)
        {
            entered = 0;
            exited = 0;

            if (!int.TryParse(_view.Entered.Text, out entered) ||
                entered < 0 || entered > 255)
            {
                MessageBox.Show("Поле 'Вошло' должно быть числом 0..255",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(_view.Exited.Text, out exited) ||
                exited < 0 || exited > 255)
            {
                MessageBox.Show("Поле 'Вышло' должно быть числом 0..255",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void OperationsStatusChanged(string status)
        {
            if (_disposed || _view.Status.IsDisposed || !_view.Status.IsHandleCreated)
                return;

            if (_view.Status.InvokeRequired)
            {
                try
                {
                    _view.Status.BeginInvoke(
                        new Action(() => UpdateStatus(status)));
                }
                catch (InvalidOperationException)
                {
                    // The form is closing.
                }
                return;
            }

            UpdateStatus(status);
        }

        private void UpdateStatus(string status)
        {
            if (_disposed || _view.Status.IsDisposed)
                return;

            _view.Status.Text = $"Статус: {status}";
            SetButtons(_operations.IsRunning);
        }

        private void SetButtons(bool running)
        {
            _view.Start.Enabled = !running;
            _view.Stop.Enabled = running;
            _view.Test.Enabled = !running;
            _view.Update.Enabled = true;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _operations.StatusChanged -= OperationsStatusChanged;
        }
    }
}
