using MKtest.Services.Demoscripts;
using System.Collections.Generic;

namespace MKtest.Services.DemoScenarios
{
    public class Demo1Scenario : IScenario
    {
        public string Name => "demo-1";
        public string Description => "Демо-сценарий для маршрута 1";
        public IReadOnlyList<ScenarioStep> Steps { get; }

        public Demo1Scenario()
        {
            var list = new List<ScenarioStep>
            {
                new ScenarioStep
                {
                    SourceFile = "1r1.json",
                    TargetFile = "route.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.Once,
                    Description = "Загрузка route 1r1"
                }
            };

            for (int i = 1; i <= 11; i++)
            {
                list.Add(new ScenarioStep
                {
                    SourceFile = $"1i{i}.json",
                    TargetFile = "informator.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = $"Остановка 1i{i}"
                });

                list.Add(new ScenarioStep
                {
                    SourceFile = $"coord1{i}.json",
                    TargetFile = "coord.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = $"Координаты coord1{i}"
                });

                list.Add(new ScenarioStep
                {
                    SourceFile = "",
                    TargetFile = "",
                    DelayMs = 20000,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = $"Задержка 20 сек"
                });
            }

            Steps = list;
        }
    }
}