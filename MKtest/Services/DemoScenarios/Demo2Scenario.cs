using System.Collections.Generic;

namespace MKtest.Services.DemoScenarios
{
    public class Demo2Scenario : IScenario
    {
        public string Name => "demo-2";
        public string Description => "Демо-сценарий для маршрута 2";
        public IReadOnlyList<ScenarioStep> Steps { get; }

        public Demo2Scenario()
        {
            var list = new List<ScenarioStep>
            {
                new ScenarioStep
                {
                    SourceFile = "2r1.json",
                    TargetFile = "route.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.Once,
                    Description = "Загрузка route 2r1"
                }
            };

            for (int i = 1; i <= 21; i++)
            {
                list.Add(new ScenarioStep
                {
                    SourceFile = $"2i{i}.json",
                    TargetFile = "informator.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = $"Остановка 2i{i}"
                });

                list.Add(new ScenarioStep
                {
                    SourceFile = "coord21.json",
                    TargetFile = "coord.json",
                    DelayMs = 0,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = "Координаты маршрута 2"
                });

                list.Add(new ScenarioStep
                {
                    SourceFile = "",
                    TargetFile = "",
                    DelayMs = 20000,
                    ExecutionMode = StepExecutionMode.PerLoop,
                    Description = "Задержка 20 сек"
                });
            }

            Steps = list;
        }
    }
}