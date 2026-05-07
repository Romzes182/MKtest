using MKtest.Services.Demoscripts;
using System.Collections.Generic;

namespace MKtest.Services.DemoScenarios
{
    public class Demo5Scenario : IScenario
    {
        public string Name => "demo-5";
        public string Description => "Демо-сценарий для маршрута 5";
        public IReadOnlyList<ScenarioStep> Steps { get; }

        public Demo5Scenario()
        {
            var list = new List<ScenarioStep>
            {
                new ScenarioStep
                {
                    SourceFile = "5r1.json",
                    TargetFile = "route.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.Once,
                    Description = "Загрузка route 5r1"
                }
            };

            for (int i = 1; i <= 1; i++)
            {
                list.Add(new ScenarioStep
                {
                    SourceFile = $"5i{i}.json",
                    TargetFile = "informator.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = $"Остановка 5i{i}"
                });

                list.Add(new ScenarioStep
                {
                    SourceFile = "coord51.json",
                    TargetFile = "coord.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = "Координаты маршрута 5"
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