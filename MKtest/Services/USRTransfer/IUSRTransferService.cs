using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.USRTransfer
{
    public interface IUSRTransferService
    {
        event EventHandler<TransferProgressEventArgs>? ProgressChanged;
        Task<bool> TestConnectionAsync();

        Task SendSvcFolderAsync(RouteConfig route);
        Task RunInSequenceAsync(RouteConfig route, CancellationToken token);

        // NEW:
        Task SendSingleFileAsync(string filePath, int baudRate, string type);
        Task RunInSequenceFromAsync(RouteConfig route, int startIndex, CancellationToken token);
    }
}