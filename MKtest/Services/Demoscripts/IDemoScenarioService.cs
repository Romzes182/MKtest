using System;
using System.Collections.Generic;

namespace MKtest.Services.Demoscripts
{
    public interface IDemoScenarioService
    {
        bool IsDemoRunning { get; }
        IReadOnlyList<string> AvailableScenarios { get; }

        void StartScenario(string scenarioName);
        void StopScenario();

        void StartScenarioManual(string scenarioName, Func<Task> waitTaskProvider);
        void StartScenarioManual(string scenarioName, Func<Task<ManualStepDirection>> navigationProvider);

        event EventHandler<string> ScenarioProgress;
        event EventHandler<bool> DemoStatusChanged;
    }
}