using System;

namespace MKtest.UI.Logging
{
    /// <summary>Обновление журнала на экране независимо от записи в файл.</summary>
    public interface IUiLogSink : IDisposable
    {
        void Append(string formattedLine);
        void Clear();
    }
}
