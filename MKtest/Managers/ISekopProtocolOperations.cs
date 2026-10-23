using System;
using System.Threading.Tasks;

namespace MKtest.Managers
{
    /// <summary>Контракт СЭКОП, используемый только представлением WinForms.</summary>
    public interface ISekopProtocolOperations
    {
        bool IsRunning { get; }
        Task StartAsync(Func<SekopValues> getValues);
        Task SendCurrentAsync();
        void Stop();
    }
}
