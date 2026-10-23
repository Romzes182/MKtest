using MKtest.Services.USRTransfer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    /// <summary>
    /// Операции ИР-0652, доступные контроллеру пользовательского интерфейса.
    /// Реализацию можно заменить без изменений в форме.
    /// </summary>
    public interface IUSRTransferOperations
    {
        event EventHandler<TransferProgressEventArgs>? ProgressChanged;
        bool IsSequenceRunning { get; }
        List<RouteConfig> LoadRoutes();
        RouteConfig? GetRoute(string routeNumber);
        Task<bool> TestConnectionAsync();
        Task SendSelectedSvcAsync(string routeNumber, string svcFileName);
        Task SendSingleInAsync(string routeNumber, string inFileName);
        Task StartInSequenceAsync(string routeNumber);
        Task StartInFromSelectedAsync(string routeNumber, string inFileName);
        void StopInSequence();
    }
}
