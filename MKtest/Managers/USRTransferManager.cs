using MKtest.Services;
using MKtest.Services.USRTransfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    public class USRTransferManager : IUSRTransferOperations
    {
        private readonly IRouteService _routeService;
        private readonly IUSRTransferService _transferService;
        private readonly LogManager _logManager;
        private CancellationTokenSource? _sequenceToken;
        private TaskCompletionSource<bool>? _sequenceComplete;
        private readonly object _sequenceSync = new();
        private List<RouteConfig> _routes = new();

        public event EventHandler<TransferProgressEventArgs>? ProgressChanged;
        public bool IsSequenceRunning { get; private set; }

        public USRTransferManager(IRouteService routeService, IUSRTransferService transferService, LogManager logManager)
        {
            _routeService = routeService;
            _transferService = transferService;
            _logManager = logManager;
            _transferService.ProgressChanged += (s, e) => ProgressChanged?.Invoke(s, e);
        }

        public List<RouteConfig> LoadRoutes()
        {
            _routes = _routeService.LoadRoutes();
            _logManager.AppendLog($"Загружено маршрутов USR: {_routes.Count}");
            return _routes;
        }

        public RouteConfig? GetRoute(string routeNumber)
        {
            return _routes.FirstOrDefault(r => r.RouteNumber == routeNumber);
        }

        public async Task SendSelectedSvcAsync(string routeNumber, string svcFileName)
        {
            var route = GetRoute(routeNumber);
            if (route == null) { _logManager.AppendLog("Маршрут не найден"); return; }

            var path = route.SvcFilePaths
                .FirstOrDefault(f => string.Equals(Path.GetFileName(f), svcFileName, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(path)) { _logManager.AppendLog("SVC файл не найден"); return; }

            await _transferService.SendSingleFileAsync(path, ConfigService.Config.USRTransfer.SvcBaudRate, "SVC");
        }

        public async Task SendSingleInAsync(string routeNumber, string inFileName)
        {
            var route = GetRoute(routeNumber);
            if (route == null) { _logManager.AppendLog("Маршрут не найден"); return; }

            var path = route.InFilePaths
                .FirstOrDefault(f => string.Equals(Path.GetFileName(f), inFileName, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(path)) { _logManager.AppendLog("IN файл не найден"); return; }

            await _transferService.SendSingleFileAsync(path, ConfigService.Config.USRTransfer.InBaudRate, "IN");
        }

        public Task StartInFromSelectedAsync(string routeNumber, string inFileName)
        {
            var route = GetRoute(routeNumber);
            if (route == null) { _logManager.AppendLog("Маршрут не найден"); return Task.CompletedTask; }

            var startIndex = route.InFilePaths.FindIndex(f =>
                string.Equals(Path.GetFileName(f), inFileName, StringComparison.OrdinalIgnoreCase));

            if (startIndex < 0) { _logManager.AppendLog("Выбранный IN файл не найден"); return Task.CompletedTask; }

            return RunSequenceAsync(route, startIndex, fromSelected: true);
        }

        public Task<bool> TestConnectionAsync() => _transferService.TestConnectionAsync();

        public async Task SendSvcAsync(string routeNumber)
        {
            var route = _routes.FirstOrDefault(r => r.RouteNumber == routeNumber);
            if (route == null) { _logManager.AppendLog("Маршрут не найден"); return; }
            await _transferService.SendSvcFolderAsync(route);
        }

        public Task StartInSequenceAsync(string routeNumber)
        {
            var route = _routes.FirstOrDefault(r => r.RouteNumber == routeNumber);
            if (route == null) { _logManager.AppendLog("Маршрут не найден"); return Task.CompletedTask; }
            return RunSequenceAsync(route, 0, fromSelected: false);
        }

        private async Task RunSequenceAsync(RouteConfig route, int startIndex, bool fromSelected)
        {
            CancellationTokenSource source;
            TaskCompletionSource<bool> finished;

            lock (_sequenceSync)
            {
                if (IsSequenceRunning)
                    return;

                IsSequenceRunning = true;
                source = new CancellationTokenSource();
                _sequenceToken = source;
                finished = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                _sequenceComplete = finished;
            }

            try
            {
                if (fromSelected)
                    await _transferService.RunInSequenceFromAsync(route, startIndex, source.Token)
                        .ConfigureAwait(false);
                else
                    await _transferService.RunInSequenceAsync(route, source.Token)
                        .ConfigureAwait(false);
            }
            finally
            {
                lock (_sequenceSync)
                {
                    IsSequenceRunning = false;
                    _sequenceToken = null;
                    _sequenceComplete = null;
                }

                source.Dispose();
                finished.TrySetResult(true);
            }
        }

        public void StopInSequence()
        {
            lock (_sequenceSync)
            {
                if (_sequenceToken == null) return;
                if (!_sequenceToken.IsCancellationRequested)
                    _sequenceToken.Cancel();
            }
            _logManager.AppendLog("Запрошена остановка последовательности IN");
        }

        public async Task StopInSequenceAsync()
        {
            StopInSequence();
            Task? pending;
            lock (_sequenceSync)
                pending = _sequenceComplete?.Task;
            if (pending != null)
                await pending.ConfigureAwait(false);
        }
    }
}
