using System;
using System.Collections.Generic;

namespace MKtest.Services
{
    public interface IDemoScenarioService
    {
        bool IsDemoRunning { get; }
        IReadOnlyList<string> AvailableScenarios { get; }

        void StartScenario(string scenarioName);
        void StopScenario();

        event EventHandler<string> ScenarioProgress;
        event EventHandler<bool> DemoStatusChanged;
    }
}