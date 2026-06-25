public interface ISettingsRepository
{
    SettingsData Load();
    void Save(SettingsData data);
}
