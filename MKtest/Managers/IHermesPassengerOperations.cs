using System;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    /// <summary>Operations required by the Hermes WinForms block.</summary>
    public interface IHermesPassengerOperations
    {
        event Action<string>? StatusChanged;
        bool IsRunning { get; }
        Task<bool> TestConnectionAsync();
        Task StartAsync(int entered, int exited);
        Task StopAsync();
        void UpdateValues(int entered, int exited);
    }
}
