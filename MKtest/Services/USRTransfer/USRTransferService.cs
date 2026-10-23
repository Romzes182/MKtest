using MKtest.Configs;
using MKtest.Managers;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.USRTransfer
{
    public class USRTransferService : IUSRTransferService
    {
        private readonly LogManager _logManager;
        public event EventHandler<TransferProgressEventArgs>? ProgressChanged;

        public USRTransferService(LogManager logManager) => _logManager = logManager;

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var client = new TcpClient();
                var cfg = ConfigService.Config.USRTransfer;
                var done = await WaitConnectAsync(client, cfg.IP, cfg.Port, 3000);
                return done;
            }
            catch
            {
                return false;
            }
        }

        public async Task SendSvcFolderAsync(RouteConfig route)
        {
            if (route.SvcFilePaths.Count == 0)
            {
                _logManager.AppendLog("SVC файлы не найдены");
                return;
            }

            foreach (var file in route.SvcFilePaths)
                await SendBytecodeAsync(file, ConfigService.Config.USRTransfer.SvcBaudRate, "SVC");
        }
        public Task SendSingleFileAsync(string filePath, int baudRate, string type)
        {
            return SendBytecodeAsync(filePath, baudRate, type);
        }
        public Task RunInSequenceAsync(RouteConfig route, CancellationToken token)
        {
            return RunInSequenceFromAsync(route, 0, token);
        }
        public async Task RunInSequenceFromAsync(RouteConfig route, int startIndex, CancellationToken token)
        {
            var total = route.InFilePaths.Count;
            if (total == 0 || startIndex < 0 || startIndex >= total)
            {
                Raise(0, total, 0, "IN файлы не найдены");
                return;
            }

            try
            {
                for (int i = startIndex; i < total; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var file = route.InFilePaths[i];
                    if (!File.Exists(file)) continue;

                    Raise(i + 1, total, 0, $"Отправка IN: {Path.GetFileName(file)}");
                    await SendBytecodeAsync(file, ConfigService.Config.USRTransfer.InBaudRate,
                        $"IN{i + 1}", token).ConfigureAwait(false);
                    await WaitBetweenStepsAsync(i, total, token);
                }

                Raise(0, total, 0, "Последовательность завершена");
            }
            catch (OperationCanceledException)
            {
                _logManager.AppendLog("IN последовательность остановлена пользователем");
                Raise(0, total, 0, "Остановлено");
            }
            catch (Exception ex)
            {
                _logManager.AppendLog($"Ошибка IN последовательности: {ex.Message}");
                Raise(0, total, 0, "Ошибка");
            }
        }
        private async Task SendBytecodeAsync(
            string filePath, int baudRate, string type,
            CancellationToken token = default)
        {
            try
            {
                var cfg = ConfigService.Config.USRTransfer;
                var fileData = await File.ReadAllBytesAsync(filePath, token);
                _logManager.AppendLog($"Отправка {type}: {Path.GetFileName(filePath)} ({fileData.Length} байт)");

                using var client = new TcpClient();
                var connected = await WaitConnectAsync(client, cfg.IP, cfg.Port, 3000, token);
                if (!connected)
                {
                    _logManager.AppendLog($"Ошибка {type}: таймаут подключения к {cfg.IP}:{cfg.Port}");
                    return;
                }

                await using var stream = client.GetStream();
                await stream.WriteAsync(USRCommands.GetBaudRateCommand(baudRate), token);
                await stream.WriteAsync(USRCommands.CreateSaveSettingsCommand(), token);
                await Task.Delay(1000, token);

                await stream.WriteAsync(fileData, token);
                await stream.FlushAsync(token);
                await Task.Delay(500, token);

                _logManager.AppendLog($"Успешно отправлен {type}: {Path.GetFileName(filePath)}");
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logManager.AppendLog($"Ошибка отправки {type}: {ex.Message}");
            }
        }

        private static async Task<bool> WaitConnectAsync(
            TcpClient client, string ip, int port, int timeoutMs,
            CancellationToken token = default)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeout.CancelAfter(timeoutMs);

            try
            {
                await client.ConnectAsync(ip, port, timeout.Token);
                return client.Connected;
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            {
                return false;
            }
        }

        private async Task WaitBetweenStepsAsync(int index, int total, CancellationToken token)
        {
            if (index >= total - 1)
            {
                await Task.Delay(ConfigService.Config.USRTransfer.DelayAfterLastInMs, token);
                return;
            }

            for (int s = ConfigService.Config.USRTransfer.DelayBetweenInSeconds; s > 0; s--)
            {
                token.ThrowIfCancellationRequested();
                Raise(index + 1, total, s, "Ожидание до следующего IN");
                await Task.Delay(1000, token);
            }
        }

        private void Raise(int current, int total, int countdown, string status)
        {
            ProgressChanged?.Invoke(this, new TransferProgressEventArgs
            {
                CurrentStep = current,
                TotalSteps = total,
                CountdownSeconds = countdown,
                Status = status
            });
        }
    }
}