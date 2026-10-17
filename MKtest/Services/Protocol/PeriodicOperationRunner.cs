using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.Protocol
{
    /// <summary>
    /// Cancels periodic runs reliably and awaits them during shutdown.
    /// One asynchronous operation at a time, even across rapid Stop/Start.
    /// </summary>
    public sealed class PeriodicOperationRunner
    {
        private sealed class RunState
        {
            public readonly CancellationTokenSource Source = new();
            public Task? Worker;
        }

        private readonly object _sync = new();
        private readonly SemaphoreSlim _operationGate = new(1, 1);
        private readonly HashSet<RunState> _runs = new();
        private RunState? _current;

        public bool IsRunning
        {
            get
            {
                lock (_sync)
                    return _current != null && !_current.Source.IsCancellationRequested;
            }
        }

        public bool Start(
            Func<CancellationToken, Task> operation,
            Func<int> intervalSeconds,
            Action<Exception> onError)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentNullException.ThrowIfNull(intervalSeconds);
            ArgumentNullException.ThrowIfNull(onError);

            lock (_sync)
            {
                if (_current != null)
                    return false;

                var run = new RunState();
                _current = run;
                _runs.Add(run);
                run.Worker = Task.Run(() => RunAsync(run, operation, intervalSeconds, onError));
                return true;
            }
        }

        private async Task RunAsync(
            RunState run,
            Func<CancellationToken, Task> operation,
            Func<int> intervalSeconds,
            Action<Exception> onError)
        {
            var token = run.Source.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    await _operationGate.WaitAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            await operation(token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException) when (token.IsCancellationRequested)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            ReportError(onError, ex);
                        }
                    }
                    finally
                    {
                        _operationGate.Release();
                    }

                    token.ThrowIfCancellationRequested();
                    int seconds = Math.Max(1, intervalSeconds());
                    await Task.Delay(TimeSpan.FromSeconds(seconds), token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception ex)
            {
                ReportError(onError, ex);
            }
            finally
            {
                lock (_sync)
                {
                    _runs.Remove(run);
                    if (ReferenceEquals(_current, run))
                        _current = null;
                }
                run.Source.Dispose();
            }
        }

        private static void ReportError(Action<Exception> callback, Exception ex)
        {
            try { callback(ex); }
            catch
            {
                // A logging callback must never fault the lifecycle worker.
            }
        }

        public void Stop()
        {
            lock (_sync)
            {
                _current = null;
                foreach (var run in _runs)
                {
                    if (!run.Source.IsCancellationRequested)
                        run.Source.Cancel();
                }
            }
        }

        /// <summary>Stop and await all in-flight runs, including canceled older runs.</summary>
        public async Task StopAsync()
        {
            Task[] pending;
            lock (_sync)
            {
                _current = null;
                foreach (var run in _runs)
                {
                    if (!run.Source.IsCancellationRequested)
                        run.Source.Cancel();
                }

                pending = _runs.Select(run => run.Worker ?? Task.CompletedTask).ToArray();
            }

            if (pending.Length > 0)
                await Task.WhenAll(pending).ConfigureAwait(false);
        }
    }
}
