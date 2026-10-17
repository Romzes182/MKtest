using MKtest.Configs;
using MKtest.Managers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.HermesPassenger
{
    /// <summary>
    /// Owns one Hermes run and its cancellation. SSH transport, each command,
    /// and the periodic schedule have separate responsibilities.
    /// </summary>
    public sealed class HermesPassengerService : IHermesPassengerService
    {
        private readonly LogManager _log;
        private readonly HermesSSHConfig _config;
        private readonly IHermesSshSessionFactory _sessionFactory;
        private readonly SemaphoreSlim _lifecycle = new(1, 1);
        private readonly object _stateSync = new();

        private IHermesSshSession? _session;
        private CancellationTokenSource? _runCancellation;
        private Task? _workers;
        private HermesPassengerValues _values = new(0, 0);
        private volatile bool _disposed;

        public event Action<string>? StatusChanged;
        public event Action<string>? ErrorOccurred;

        public bool IsRunning
        {
            get
            {
                lock (_stateSync)
                    return _runCancellation is { IsCancellationRequested: false };
            }
        }

        // Compatibility: the constructor used by MainForm is unchanged.
        public HermesPassengerService(LogManager log, HermesSSHConfig config)
            : this(log, config, new HermesSshSessionFactory()) { }

        public HermesPassengerService(
            LogManager log,
            HermesSSHConfig config,
            IHermesSshSessionFactory sessionFactory)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        }

        public async Task<bool> TestConnectionAsync()
        {
            if (_disposed) return false;
            _log.AppendLog($"Hermes test: {_config.IP}:{_config.Port}, user={_config.User}");

            IHermesSshSession? testSession = null;
            try
            {
                // A test gets its own SSH client; it never interrupts the active run.
                testSession = _sessionFactory.Create(_config);
                await testSession.ConnectAsync(CancellationToken.None).ConfigureAwait(false);
                return testSession.IsConnected;
            }
            catch (Exception ex)
            {
                _log.AppendLog($"Hermes test error: {ex.Message}");
                return false;
            }
            finally
            {
                if (testSession != null)
                {
                    try { await testSession.DisconnectAsync().ConfigureAwait(false); }
                    catch (Exception ex) { _log.AppendLog($"Hermes test cleanup: {ex.Message}"); }
                    testSession.Dispose();
                }
            }
        }

        public async Task StartAsync(int entered, int exited)
        {
            await _lifecycle.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_disposed || _runCancellation != null)
                    return;

                SetValues(entered, exited);
                IHermesSshSession session;
                try
                {
                    session = _sessionFactory.Create(_config);
                }
                catch (Exception ex)
                {
                    _log.AppendLog($"Гермес: ошибка создания SSH клиента: {ex.Message}");
                    ErrorOccurred?.Invoke(ex.Message);
                    return;
                }

                var source = new CancellationTokenSource();
                lock (_stateSync)
                {
                    _runCancellation = source;
                    _session = session;
                }

                try
                {
                    _log.AppendLog($"Hermes connect: {_config.IP}:{_config.Port}, user={_config.User}");
                    await session.ConnectAsync(source.Token).ConfigureAwait(false);
                    source.Token.ThrowIfCancellationRequested();
                    _log.AppendLog($"Гермес: подключено к {_config.IP}:{_config.Port}");

                    var kill = new HermesKillProcessTask(session, _log.AppendLog);
                    var file = new HermesPassengerFileTask(
                        session, _config, () => Volatile.Read(ref _values), _log.AppendLog);
                    var killWorker = new HermesPeriodicWorker(
                        _config.KillIntervalSeconds, kill.ExecuteAsync,
                        "Гермес kill-loop ошибка", _log.AppendLog);
                    var fileWorker = new HermesPeriodicWorker(
                        _config.FileUpdateIntervalSeconds, file.ExecuteAsync,
                        "Гермес file-loop ошибка", _log.AppendLog);

                    _workers = Task.WhenAll(
                        killWorker.RunAsync(source.Token),
                        fileWorker.RunAsync(source.Token));
                    _ = ObserveWorkersAsync(_workers);

                    _log.AppendLog("Гермес: SSH управление запущено");
                    StatusChanged?.Invoke("АКТИВНО");
                }
                catch (OperationCanceledException) when (source.IsCancellationRequested)
                {
                    await CleanupUnderLockAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _log.AppendLog($"Гермес: ошибка запуска: {ex.Message}");
                    ErrorOccurred?.Invoke(ex.Message);
                    await CleanupUnderLockAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                _lifecycle.Release();
            }
        }

        public async Task StopAsync()
        {
            // Signal cancellation BEFORE waiting for StartAsync's lifecycle gate.
            // This also cancels a run if Stop is pressed during SSH connect.
            lock (_stateSync)
            {
                if (_runCancellation != null)
                {
                    try { _runCancellation.Cancel(); }
                    catch (ObjectDisposedException) { }
                }
            }

            await _lifecycle.WaitAsync().ConfigureAwait(false);
            try
            {
                await CleanupUnderLockAsync().ConfigureAwait(false);
            }
            finally
            {
                _lifecycle.Release();
            }
        }

        public void SetValues(int entered, int exited)
        {
            // A single immutable snapshot keeps A and B from different updates
            // from being mixed by the periodic file worker.
            Volatile.Write(ref _values, new HermesPassengerValues(entered, exited));
            _log.AppendLog($"Гермес: значения обновлены A={entered}, B={exited}");
        }

        private async Task ObserveWorkersAsync(Task runningWorkers)
        {
            try
            {
                await runningWorkers.ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _log.AppendLog($"Гермес: ошибка фоновых задач: {ex.Message}");
                ErrorOccurred?.Invoke(ex.Message);
            }

            // On unexpected completion, release the active session as well.
            await StopAsync().ConfigureAwait(false);
        }

        // Must be called while _lifecycle is held.
        private async Task CleanupUnderLockAsync()
        {
            CancellationTokenSource? source;
            IHermesSshSession? session;
            Task? workers;

            lock (_stateSync)
            {
                source = _runCancellation;
                session = _session;
                workers = _workers;
                _runCancellation = null;
                _session = null;
                _workers = null;
            }

            if (source == null && session == null)
                return;

            try
            {
                if (source != null && !source.IsCancellationRequested)
                    source.Cancel();

                if (workers != null)
                {
                    try { await workers.ConfigureAwait(false); }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        _log.AppendLog($"Гермес: завершение задач: {ex.Message}");
                    }
                }

                if (session != null)
                {
                    try { await session.DisconnectAsync().ConfigureAwait(false); }
                    catch (Exception ex)
                    {
                        _log.AppendLog($"Гермес: отключение SSH: {ex.Message}");
                    }
                }
            }
            finally
            {
                try { session?.Dispose(); }
                catch (Exception ex)
                {
                    _log.AppendLog($"Гермес: освобождение SSH: {ex.Message}");
                }
                source?.Dispose();
                _log.AppendLog("Гермес: SSH управление остановлено");
                StatusChanged?.Invoke("ОСТАНОВЛЕНО");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            // All awaits in this service avoid capturing the WinForms context.
            StopAsync().GetAwaiter().GetResult();
        }
    }
}
