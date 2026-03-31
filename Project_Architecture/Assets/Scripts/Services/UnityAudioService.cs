using UnityEngine;

public sealed class UnityAudioService : IAudioService
{
    private const string SfxVolumeKey = "settings.sfx_volume";
    private float sfxVolume;

    public UnityAudioService()
    {
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        Apply();
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
            PlayerPrefs.Save();
            Apply();
        }
    }

    private void Apply()
    {
        AudioListener.volume = sfxVolume;
    }
}
