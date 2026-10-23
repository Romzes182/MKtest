using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.HermesPassenger
{
    /// <summary>One serialized SSH connection for executing Hermes commands.</summary>
    public interface IHermesSshSession : IDisposable
    {
        bool IsConnected { get; }
        Task ConnectAsync(CancellationToken token);
        Task<bool> ExecuteAsync(string command, CancellationToken token);
        Task DisconnectAsync();
    }
}
