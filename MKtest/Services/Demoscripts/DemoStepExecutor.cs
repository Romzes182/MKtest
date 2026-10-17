using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.Demoscripts
{
    /// <summary>
    /// Копирование JSON, задержки и ожидание ручного подтверждения отдельного шага.
    /// Доступ к файловой системе делегируется IDemoFileService.
    /// </summary>
    public sealed class DemoStepExecutor : IDemoStepExecutor
    {
        private readonly IDemoFileService _files;
        private readonly Action<string> _log;

        public DemoStepExecutor(IDemoFileService files, Action<string> log)
        {
            _files = files ?? throw new ArgumentNullException(nameof(files));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task ExecuteAsync(
            ScenarioStep step,
            string scenarioName,
            DemoRunOptions options,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                bool delayOnly = string.IsNullOrEmpty(step.SourceFile) && step.DelayMs > 0;

                // Manual mode skips standalone pauses just like the original code.
                if (options.ManualMode && delayOnly)
                    return;

                if (!string.IsNullOrEmpty(step.SourceFile) &&
                    !string.IsNullOrEmpty(step.TargetFile))
                {
                    await CopyScenarioFileAsync(step, scenarioName, options, cancellationToken);

                    if (options.ManualMode && options.WaitForNext != null)
                    {
                        options.ReportProgress("Ожидание нажатия кнопки 'Далее'...");
                        // Cancel promptly if Stop is pressed while waiting for Next.
                        await options.WaitForNext().WaitAsync(cancellationToken);
                    }
                }
                else if (!options.ManualMode && step.DelayMs > 0)
                {
                    options.ReportProgress($"Задержка {step.DelayMs}мс: {step.Description}");
                    await Task.Delay(step.DelayMs, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Keep the original policy: log a failed step and proceed to the next.
                _log($"Ошибка выполнения шага '{step.Description}': {ex.Message}");
                options.ReportProgress($"Ошибка: {step.Description}");
            }
        }

        private async Task CopyScenarioFileAsync(
            ScenarioStep step,
            string scenarioName,
            DemoRunOptions options,
            CancellationToken cancellationToken)
        {
            try
            {
                var scenarioPath = _files.GetScenarioPath(scenarioName);
                var sourcePath = Path.Combine(scenarioPath, step.SourceFile);
                var destPath = Path.Combine("JsonTemplates", step.TargetFile);

                cancellationToken.ThrowIfCancellationRequested();
                await _files.CopyFileAsync(sourcePath, destPath);
                cancellationToken.ThrowIfCancellationRequested();

                options.ReportProgress(step.Description);
                _log($"[Демо] {step.Description}");
            }
            catch (FileNotFoundException ex)
            {
                _log($"Файл не найден: {ex.FileName}");
                throw;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _log($"Ошибка копирования файла: {ex.Message}");
                throw;
            }
        }
    }
}
