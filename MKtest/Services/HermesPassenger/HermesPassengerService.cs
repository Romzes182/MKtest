using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MKtest.Configs;
using MKtest.Managers;
using Renci.SshNet;
using System.Net.NetworkInformation;

namespace MKtest.Services.HermesPassenger
{
    public class HermesPassengerService : IHermesPassengerService
    {
        private readonly LogManager _log;
        private readonly HermesSSHConfig _cfg;
        private SshClient? _ssh;
        private CancellationTokenSource? _cts;
        private bool _isRunning;
        private int _entered;
        private int _exited;

        public event Action<string>? StatusChanged;
        public event Action<string>? ErrorOccurred;
        public bool IsRunning => _isRunning;

        public HermesPassengerService(LogManager log, HermesSSHConfig cfg)
        {
            _log = log;
            _cfg = cfg;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _log.AppendLog($"Hermes test: {_cfg.IP}:{_cfg.Port}, user={_cfg.User}");
                using var ssh = new SshClient(CreateConnectionInfo());
                await Task.Run(() => ssh.Connect());
                var ok = ssh.IsConnected;
                if (ok) ssh.Disconnect();
                return ok;
            }
            catch (Exception ex)
            {
                _log.AppendLog($"Hermes test error: {ex.Message}");
                return false;
            }
        }
        public async Task StartAsync(int entered, int exited)
        {
            if (_isRunning) return;

            _entered = entered;
            _exited = exited;
            _cts = new CancellationTokenSource();
            _isRunning = true;

            try
            {
                await ConnectAsync();
                _ = Task.Run(() => KillLoopAsync(_cts.Token));
                _ = Task.Run(() => FileLoopAsync(_cts.Token));

                _log.AppendLog("Гермес: SSH управление запущено");
                StatusChanged?.Invoke("АКТИВНО");
            }
            catch (Exception ex)
            {
                _log.AppendLog($"Гермес: ошибка запуска: {ex.Message}");
                ErrorOccurred?.Invoke(ex.Message);
                await StopAsync();
            }
        }

        public Task StopAsync()
        {
            _isRunning = false;

            if (_cts != null && !_cts.IsCancellationRequested)
                _cts.Cancel();

            try
            {
                if (_ssh?.IsConnected == true) _ssh.Disconnect();
                _ssh?.Dispose();
            }
            catch { }

            _ssh = null;
            _log.AppendLog("Гермес: SSH управление остановлено");
            StatusChanged?.Invoke("ОСТАНОВЛЕНО");
            return Task.CompletedTask;
        }

        public void SetValues(int entered, int exited)
        {
            _entered = entered;
            _exited = exited;
            _log.AppendLog($"Гермес: значения обновлены A={entered}, B={exited}");
        }

        private async Task ConnectAsync()
        {
            _log.AppendLog($"Hermes connect: {_cfg.IP}:{_cfg.Port}, user={_cfg.User}");

            _ssh = new SshClient(CreateConnectionInfo());
            await Task.Run(() => _ssh.Connect());

            if (_ssh?.IsConnected != true)
                throw new Exception("Не удалось установить SSH соединение");

            _log.AppendLog($"Гермес: подключено к {_cfg.IP}:{_cfg.Port}");
        }
        private async Task KillLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _isRunning)
            {
                try
                {
                    if (_ssh?.IsConnected == true)
                    {
                        await Task.Run(() => _ssh.RunCommand("killall pfsens"));
                        _log.AppendLog("Гермес: выполнен killall pfsens");
                    }
                }
                catch (Exception ex)
                {
                    _log.AppendLog($"Гермес kill-loop ошибка: {ex.Message}");
                }

                await Task.Delay(_cfg.KillIntervalSeconds * 1000, token);
            }
        }

        private async Task FileLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _isRunning)
            {
                try
                {
                    if (_ssh?.IsConnected == true)
                    {
                        var content = $"{_entered};{_exited};61440";
                        var cmd = $"echo '{content}' > {_cfg.RemoteFilePath}";
                        await Task.Run(() => _ssh.RunCommand(cmd));
                        _log.AppendLog($"Гермес: файл обновлён {content}");
                    }
                }
                catch (Exception ex)
                {
                    _log.AppendLog($"Гермес file-loop ошибка: {ex.Message}");
                }

                await Task.Delay(_cfg.FileUpdateIntervalSeconds * 1000, token);
            }
        }

        private ConnectionInfo CreateConnectionInfo()
        {
            return new ConnectionInfo(
                _cfg.IP,
                _cfg.Port,
                _cfg.User,
                new PasswordAuthenticationMethod(_cfg.User, _cfg.Password)
            )
            { Timeout = TimeSpan.FromSeconds(30) };
        }

     

        public void Dispose()
        {
            try { StopAsync().Wait(3000); } catch { }
            _cts?.Dispose();
            _ssh?.Dispose();
        }
    }
}