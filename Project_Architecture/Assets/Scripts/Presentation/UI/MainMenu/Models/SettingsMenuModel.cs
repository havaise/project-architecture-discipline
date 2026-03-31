public sealed class SettingsMenuModel
{
    private readonly IAudioService audioService;

    public SettingsMenuModel(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    public float SfxVolume
    {
        get => audioService.SfxVolume;
        set => audioService.SfxVolume = value;
    }
}
