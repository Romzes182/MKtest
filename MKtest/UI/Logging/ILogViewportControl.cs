namespace MKtest.UI.Logging
{
    /// <summary>
    /// Управление режимом чтения журнала независимо от записи сообщений.
    /// </summary>
    public interface ILogViewportControl
    {
        void ResumeToLatest();
    }
}
