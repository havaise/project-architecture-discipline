using UnityEngine;

public sealed class UnityAudioService : IAudioService
{
    private float sfxVolume = 1f;

    public float SfxVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            AudioListener.volume = sfxVolume;
        }
    }
}
