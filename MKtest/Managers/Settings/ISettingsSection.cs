namespace MKtest.Managers.Settings
{
    /// <summary>
    /// Одна группа настроек. Apply переносит значения UI в память;
    /// запись config.json выполняется отдельно координатором.
    /// </summary>
    public interface ISettingsSection
    {
        void Load();
        void Apply();
        void Reset();
    }
}
