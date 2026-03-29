using UnityEngine;

public sealed class UnityAudioService : IAudioService
{
    private const string MusicVolumePrefKey = "settings.music_volume";
    private float musicVolume;

    public UnityAudioService()
    {
        musicVolume = PlayerPrefs.GetFloat(MusicVolumePrefKey, 1f);
        ApplyVolume();
    }

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicVolumePrefKey, musicVolume);
            PlayerPrefs.Save();
            ApplyVolume();
        }
    }

    private void ApplyVolume()
    {
        AudioListener.volume = musicVolume;
    }
}
