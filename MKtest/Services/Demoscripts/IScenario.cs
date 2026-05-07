using System.Collections.Generic;

namespace MKtest.Services.Demoscripts
{
    public interface IScenario
    {
        string Name { get; }
        string Description { get; }
        IReadOnlyList<ScenarioStep> Steps { get; }
    }
}