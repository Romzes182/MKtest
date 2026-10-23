namespace MKtest.Services.SSH
{
    /// <summary>
    /// Команды удалённого устройства без управления жизненным циклом SSH.
    /// Сервис времени зависит только от этих операций.
    /// </summary>
    public interface ISshCommandExecutor
    {
        string ExecuteCommand(string command);
        string ExecuteDirectCommand(string command);
    }
}
