using UnityEngine;

public sealed class SettingsRepository : ISettingsRepository
{
    private const string SfxVolumeKey = "settings.sfx_volume";

    public SettingsData Load()
    {
        return new SettingsData
        {
            SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f)
        }.Clamp();
    }

    public void Save(SettingsData data)
    {
        SettingsData value = (data ?? new SettingsData()).Clamp();
        PlayerPrefs.SetFloat(SfxVolumeKey, value.SfxVolume);
        PlayerPrefs.Save();
    }
}
