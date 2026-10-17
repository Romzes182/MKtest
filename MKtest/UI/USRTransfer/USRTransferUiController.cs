using MKtest.Managers;
using MKtest.Services.USRTransfer;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKtest.UI.USRTransfer
{
    /// <summary>
    /// Ссылки на существующие элементы WinForms. Контроллер не создаёт и не
    /// переименовывает их — файл Designer продолжает обслуживаться Visual Studio.
    /// </summary>
    public sealed record USRTransferControls(
        ComboBox Routes,
        ComboBox Mode,
        ListBox SvcFiles,
        ListBox InFiles,
        Button Test,
        Button SendSvc,
        Button StartIn,
        Button StopIn,
        Label Status,
        Label Step,
        Label Countdown);

    /// <summary>
    /// Только логика представления ИР-0652: выбор файлов, кнопки и прогресс.
    /// Сетевой обмен и очередность отправки остаются в IUSRTransferOperations.
    /// </summary>
    public sealed class USRTransferUiController : IDisposable
    {
        private readonly IUSRTransferOperations _operations;
        private readonly USRTransferControls _view;
        private readonly Action<string> _log;
        private bool _disposed;

        public USRTransferUiController(
            IUSRTransferOperations operations,
            USRTransferControls view,
            Action<string> log)
        {
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _operations.ProgressChanged += OperationsProgressChanged;
            Initialize();
        }

        private void Initialize()
        {
            _view.Mode.Items.Clear();
            _view.Mode.Items.AddRange(new object[]
            {
                "Все подряд",
                "С выбранного",
                "Только выбранный"
            });
            _view.Mode.SelectedIndex = 0;

            _view.Routes.Items.Clear();
            foreach (var route in _operations.LoadRoutes())
                _view.Routes.Items.Add(route.RouteNumber);
            if (_view.Routes.Items.Count > 0)
                _view.Routes.SelectedIndex = 0;

            RouteChanged();
            SetButtons(false);
            _view.Status.Text = "Статус: Остановлен";
            _view.Step.Text = "Шаг: 0/0";
            _view.Countdown.Text = "След. через: -- сек";
        }

        public void RouteChanged()
        {
            if (_disposed) return;

            _view.SvcFiles.Items.Clear();
            _view.InFiles.Items.Clear();

            var routeNumber = _view.Routes.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(routeNumber)) return;

            // Маршруты загружаются при инициализации, а не при каждом выборе.
            var route = _operations.GetRoute(routeNumber);
            if (route == null) return;

            foreach (var path in route.SvcFilePaths)
                _view.SvcFiles.Items.Add(Path.GetFileName(path));
            foreach (var path in route.InFilePaths)
                _view.InFiles.Items.Add(Path.GetFileName(path));

            _view.Status.Text = $"Статус: Маршрут {route.RouteNumber}";
            _view.Step.Text = $"Шаг: 0/{route.InFilePaths.Count}";
            _view.Countdown.Text = "След. через: -- сек";
        }

        public async Task TestConnectionAsync()
        {
            if (_disposed) return;
            try
            {
                var connected = await _operations.TestConnectionAsync();
                _log(connected ? "USR: подключение установлено" : "USR: тест не пройден (timeout/ошибка)");
            }
            catch (Exception ex)
            {
                _log($"USR: ошибка проверки подключения: {ex.Message}");
            }
        }

        public async Task SendSelectedSvcAsync()
        {
            if (_disposed) return;

            var route = _view.Routes.SelectedItem?.ToString();
            var file = _view.SvcFiles.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(route) || string.IsNullOrWhiteSpace(file))
            {
                _log("Выберите маршрут и SVC файл");
                return;
            }

            try
            {
                await _operations.SendSelectedSvcAsync(route, file);
            }
            catch (Exception ex)
            {
                _log($"USR: ошибка отправки SVC: {ex.Message}");
            }
        }

        public async Task StartInAsync()
        {
            if (_disposed || _operations.IsSequenceRunning) return;

            var route = _view.Routes.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(route)) return;

            var mode = _view.Mode.SelectedItem?.ToString() ?? "Все подряд";
            var file = _view.InFiles.SelectedItem?.ToString() ?? string.Empty;

            if (mode != "Все подряд" && string.IsNullOrWhiteSpace(file))
            {
                _log(mode == "С выбранного"
                    ? "Выберите IN файл для режима 'С выбранного'"
                    : "Выберите IN файл для режима 'Только выбранный'");
                return;
            }

            SetButtons(true);
            try
            {
                switch (mode)
                {
                    case "Все подряд":
                        await _operations.StartInSequenceAsync(route);
                        break;
                    case "С выбранного":
                        await _operations.StartInFromSelectedAsync(route, file);
                        break;
                    default:
                        await _operations.SendSingleInAsync(route, file);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                _log("USR: последовательность остановлена");
            }
            catch (Exception ex)
            {
                _log($"USR: ошибка передачи IN: {ex.Message}");
            }
            finally
            {
                if (!_disposed && !_view.StartIn.IsDisposed)
                {
                    SetButtons(false);
                    _view.Countdown.Text = "След. через: -- сек";
                }
            }
        }

        public void StopIn() => _operations.StopInSequence();

        private void OperationsProgressChanged(object? sender, TransferProgressEventArgs progress)
        {
            if (_disposed || _view.Status.IsDisposed || !_view.Status.IsHandleCreated) return;

            if (_view.Status.InvokeRequired)
            {
                try
                {
                    _view.Status.BeginInvoke(new Action(() => UpdateProgress(progress)));
                }
                catch (InvalidOperationException) { /* Форма закрывается. */ }
                return;
            }

            UpdateProgress(progress);
        }

        private void UpdateProgress(TransferProgressEventArgs progress)
        {
            if (_disposed || _view.Status.IsDisposed) return;
            _view.Status.Text = $"Статус: {progress.Status}";
            _view.Step.Text = $"Шаг: {progress.CurrentStep}/{progress.TotalSteps}";
            _view.Countdown.Text = progress.CountdownSeconds > 0
                ? $"След. через: {progress.CountdownSeconds} сек"
                : "След. через: -- сек";
        }

        private void SetButtons(bool running)
        {
            _view.StartIn.Enabled = !running;
            _view.SendSvc.Enabled = !running;
            _view.StopIn.Enabled = running;
            _view.Test.Enabled = !running;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _operations.ProgressChanged -= OperationsProgressChanged;
        }
    }
}
