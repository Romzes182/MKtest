using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.Protocol
{
    /// <summary>
    /// Runs one periodic asynchronous operation at a time.
    /// Stop cancels the current delay or request; a new run starts after cleanup.
    /// </summary>
    public sealed class PeriodicOperationRunner
    {
        private readonly object _sync = new();
        private CancellationTokenSource? _cts;

        public bool IsRunning
        {
            get
            {
                lock (_sync) return _cts != null;
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

            CancellationTokenSource source;
            lock (_sync)
            {
                if (_cts != null) return false;
                source = new CancellationTokenSource();
                _cts = source;
            }

            _ = Task.Run(() => RunAsync(source, operation, intervalSeconds, onError));
            return true;
        }

        private async Task RunAsync(
            CancellationTokenSource source,
            Func<CancellationToken, Task> operation,
            Func<int> intervalSeconds,
            Action<Exception> onError)
        {
            try
            {
                while (!source.IsCancellationRequested)
                {
                    try
                    {
                        await operation(source.Token);
                    }
                    catch (OperationCanceledException) when (source.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        onError(ex);
                    }

                    int seconds = Math.Max(1, intervalSeconds());
                    await Task.Delay(TimeSpan.FromSeconds(seconds), source.Token);
                }
            }
            catch (OperationCanceledException) when (source.IsCancellationRequested) { }
            finally
            {
                lock (_sync)
                {
                    if (ReferenceEquals(_cts, source))
                        _cts = null;
                }
                source.Dispose();
            }
        }

        public void Stop()
        {
            lock (_sync)
            {
                if (_cts == null || _cts.IsCancellationRequested)
                    return;
                _cts.Cancel();
            }
        }
    }
}
