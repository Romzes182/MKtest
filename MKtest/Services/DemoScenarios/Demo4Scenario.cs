using MKtest.Services.Demoscripts;
using System.Collections.Generic;

namespace MKtest.Services.DemoScenarios
{
    public class Demo4Scenario : IScenario
    {
        public string Name => "demo-4";
        public string Description => "Демо-сценарий для маршрута 4";
        public IReadOnlyList<ScenarioStep> Steps { get; }

        public Demo4Scenario()
        {
            var list = new List<ScenarioStep>
            {
                new ScenarioStep
                {
                    SourceFile = "4r1.json",
                    TargetFile = "route.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.Once,
                    Description = "Загрузка route 4r1"
                }
            };

            for (int i = 1; i <= 10; i++)
            {
                list.Add(new ScenarioStep
                {
                    SourceFile = $"4i{i}.json",
                    TargetFile = "informator.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = $"Остановка 4i{i}"
                });

                list.Add(new ScenarioStep
                {
                    SourceFile = $"coord4{i}.json",
                    TargetFile = "coord.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = $"Координаты coord4{i}"
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