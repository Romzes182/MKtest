using System.Threading.Tasks;

namespace MKtest.Managers
{
    /// <summary>Four original MChS commands, with their configured URLs.</summary>
    public interface IEmergencyOperations
    {
        Task SendCommand1Async();
        Task SendCommand2Async();
        Task SendCommand3Async();
        Task SendCommand4Async();
    }
}
