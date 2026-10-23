using System.Collections.Generic;

namespace MKtest.Services.Demoscripts
{
    /// <summary>
    /// Каталог доступных сценариев. Новые наборы сценариев можно подменять
    /// независимо от механизма выполнения.
    /// </summary>
    public interface IDemoScenarioCatalog
    {
        IReadOnlyList<string> Names { get; }
        bool TryGet(string name, out IScenario scenario);
    }
}
