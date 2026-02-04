using System.Collections.Generic;

namespace MKtest.Services
{
    public interface IScenario
    {
        string Name { get; }
        string Description { get; }
        IReadOnlyList<ScenarioStep> Steps { get; }
    }
}