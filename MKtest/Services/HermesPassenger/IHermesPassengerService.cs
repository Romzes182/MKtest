using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MKtest.Services.HermesPassenger
{
    public interface IHermesPassengerService : IDisposable
    {
        event Action<string>? StatusChanged;
        event Action<string>? ErrorOccurred;
        bool IsRunning { get; }

        Task StartAsync(int entered, int exited);
        Task StopAsync();
        Task<bool> TestConnectionAsync();
        void SetValues(int entered, int exited);
    }
}