using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.HermesPassenger
{
    /// <summary>
    /// One periodic job with immediate first execution, independent of the
    /// command type. Cancellation is normal, not an unobserved task failure.
    /// </summary>
    public sealed class HermesPeriodicWorker
    {
        private readonly int _seconds;
        private readonly Func<CancellationToken, Task> _execute;
        private readonly Action<string> _log;
        private readonly string _errorPrefix;

        public HermesPeriodicWorker(
            int intervalSeconds,
            Func<CancellationToken, Task> execute,
            string errorPrefix,
            Action<string> log)
        {
            _seconds = Math.Max(1, intervalSeconds);
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _errorPrefix = errorPrefix ?? throw new ArgumentNullException(nameof(errorPrefix));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await _execute(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _log($"{_errorPrefix}: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(_seconds), token).ConfigureAwait(false);
            }
        }
    }
}
