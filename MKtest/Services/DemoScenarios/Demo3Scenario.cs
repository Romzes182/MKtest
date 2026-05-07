using MKtest.Services.Demoscripts;
using System.Collections.Generic;

namespace MKtest.Services.DemoScenarios
{
    public class Demo3Scenario : IScenario
    {
        public string Name => "demo-3";
        public string Description => "Демо-сценарий для маршрута 3";
        public IReadOnlyList<ScenarioStep> Steps { get; }

        public Demo3Scenario()
        {
            var list = new List<ScenarioStep>
            {
                new ScenarioStep
                {
                    SourceFile = "3r1.json",
                    TargetFile = "route.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.Once,
                    Description = "Загрузка route 3r1"
                }
            };

            for (int i = 1; i <= 2; i++)
            {
                list.Add(new ScenarioStep
                {
                    SourceFile = $"3i{i}.json",
                    TargetFile = "informator.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = $"Остановка 3i{i}"
                });

                list.Add(new ScenarioStep
                {
                    SourceFile = "coord31.json",
                    TargetFile = "coord.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = "Координаты маршрута 3"
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