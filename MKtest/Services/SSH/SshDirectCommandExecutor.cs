using Renci.SshNet;
using System;

namespace MKtest.Services.SSH
{
    /// <summary>
    /// Выполнение SSH exec-команд отдельно от интерактивного shell.
    /// Как и раньше, команда выполняется в исходной SSH-сессии пользователя.
    /// </summary>
    public static class SshDirectCommandExecutor
    {
        public static string Execute(SshClient client, string command, Action<string> log)
        {
            try
            {
                using var cmd = client.CreateCommand(command);
                cmd.CommandTimeout = TimeSpan.FromSeconds(10);

                log($"Выполнение команды: {command}");
                string result = cmd.Execute();
                string error = cmd.Error;

                if (!string.IsNullOrEmpty(error))
                {
                    log($"Ошибка команды: {error}");
                    return $"{result}\nERROR: {error}";
                }

                return result.Trim();
            }
            catch (Exception ex)
            {
                log($"Ошибка выполнения команды: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }
    }
}
