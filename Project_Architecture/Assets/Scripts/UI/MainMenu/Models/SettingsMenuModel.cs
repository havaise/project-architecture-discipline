public sealed class SettingsMenuModel
{
    private readonly IAudioService audioService;

    public SettingsMenuModel(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    public float MusicVolume
    {
        get => audioService.MusicVolume;
        set => audioService.MusicVolume = value;
    }
}
