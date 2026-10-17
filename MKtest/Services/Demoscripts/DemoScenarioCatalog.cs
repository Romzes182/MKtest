using MKtest.Services.DemoScenarios;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MKtest.Services.Demoscripts
{
    /// <summary>Регистрирует сценарии, не выполняя их шаги.</summary>
    public sealed class DemoScenarioCatalog : IDemoScenarioCatalog
    {
        private readonly Dictionary<string, IScenario> _scenarios;

        public DemoScenarioCatalog()
            : this(new IScenario[]
            {
                new Demo1Scenario(),
                new Demo2Scenario(),
                new Demo3Scenario(),
                new Demo4Scenario(),
                new Demo5Scenario(),
                new Demo45Scenario()
            })
        {
        }

        public DemoScenarioCatalog(IEnumerable<IScenario> scenarios)
        {
            ArgumentNullException.ThrowIfNull(scenarios);
            _scenarios = scenarios.ToDictionary(s => s.Name, StringComparer.Ordinal);
        }

        public IReadOnlyList<string> Names => _scenarios.Keys.ToArray();

        public bool TryGet(string name, out IScenario scenario) =>
            _scenarios.TryGetValue(name, out scenario!);
    }
}
