using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.Demoscripts
{
    /// <summary>Выполнение одного шага без управления повторными циклами.</summary>
    public interface IDemoStepExecutor
    {
        Task ExecuteAsync(
            ScenarioStep step,
            string scenarioName,
            DemoRunOptions options,
            CancellationToken cancellationToken);
    }
}
