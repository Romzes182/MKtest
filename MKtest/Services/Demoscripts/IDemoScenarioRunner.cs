using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.Demoscripts
{
    /// <summary>Порядок одноразовых и повторяемых шагов.</summary>
    public interface IDemoScenarioRunner
    {
        Task RunAsync(
            IScenario scenario,
            DemoRunOptions options,
            CancellationToken cancellationToken);
    }
}
