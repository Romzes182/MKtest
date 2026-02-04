using System.Collections.Generic;

namespace MKtest.Services.DemoScenarios
{
    public class Demo45Scenario : IScenario
    {
        public string Name => "demo-45";
        public string Description => "Демо-сценарий для маршрута 45";
        public IReadOnlyList<ScenarioStep> Steps { get; }

        public Demo45Scenario()
        {
            var list = new List<ScenarioStep>
            {
                new ScenarioStep
                {
                    SourceFile = "45r1.json",
                    TargetFile = "route.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.Once,
                    Description = "Загрузка route 45r1"
                }
            };

            for (int i = 1; i <= 6; i++)
            {
                list.Add(new ScenarioStep
                {
                    SourceFile = $"45i{i}.json",
                    TargetFile = "informator.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = $"Остановка 45i{i}"
                });

                list.Add(new ScenarioStep
                {
                    SourceFile = "",
                    TargetFile = "",
                    DelayMs = 10000,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = "Задержка 10 сек"
                });
            }

            Steps = list;
        }
    }
}