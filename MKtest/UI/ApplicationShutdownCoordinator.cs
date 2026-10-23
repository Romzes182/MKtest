using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MKtest.UI
{
    /// <summary>
    /// Coordinates shutdown in three phases: cancel all, await workers, dispose
    /// transports. An error in one module cannot skip the remaining modules.
    /// Independent of any specific protocol, service, or WinForms control.
    /// </summary>
    public sealed class ApplicationShutdownCoordinator
    {
        private readonly Action<string> _log;
        private readonly List<(string Name, Action Stop)> _cancellation = new();
        private readonly List<(string Name, Func<Task> Wait)> _wait = new();
        private readonly List<(string Name, Func<Task> Dispose)> _disposal = new();

        public ApplicationShutdownCoordinator(Action<string> log) =>
            _log = log ?? throw new ArgumentNullException(nameof(log));

        public void AddCancel(string name, Action stop) =>
            _cancellation.Add((name, stop ?? throw new ArgumentNullException(nameof(stop))));

        public void AddWait(string name, Func<Task> wait) =>
            _wait.Add((name, wait ?? throw new ArgumentNullException(nameof(wait))));

        public void AddDispose(string name, Func<Task> dispose) =>
            _disposal.Add((name, dispose ?? throw new ArgumentNullException(nameof(dispose))));

        public async Task RunAsync()
        {
            SafeLog("Завершение работы: остановка протоколов...");

            foreach (var step in _cancellation)
            {
                try { step.Stop(); }
                catch (Exception ex) { SafeLog($"Остановка {step.Name}: {ex.Message}"); }
            }

            // Wait in parallel; all workers are allowed to finish even when
            // one of them throws. Network resources still exist at this point.
            await Task.WhenAll(_wait.Select(step => RunStepAsync(step.Name, step.Wait)))
                .ConfigureAwait(false);

            // Dispose only after cancellation has had time to drain.
            foreach (var step in _disposal)
                await RunStepAsync(step.Name, step.Dispose).ConfigureAwait(false);
        }

        private async Task RunStepAsync(string name, Func<Task> action)
        {
            try { await action().ConfigureAwait(false); }
            catch (Exception ex) { SafeLog($"Остановка {name}: {ex.Message}"); }
        }

        private void SafeLog(string message)
        {
            try { _log(message); }
            catch
            {
                // Shutdown must continue even if the log file is unavailable.
            }
        }
    }
}
