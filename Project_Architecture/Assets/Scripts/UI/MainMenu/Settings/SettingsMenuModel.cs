public sealed class SettingsMenuModel
{
    private readonly IAudioService audioService;
    private readonly ISettingsRepository settingsRepository;
    private readonly SettingsData settingsData;

    public SettingsMenuModel(IAudioService audioService, ISettingsRepository settingsRepository)
    {
        this.audioService = audioService;
        this.settingsRepository = settingsRepository;

        settingsData = settingsRepository.Load().Clamp();
        this.audioService.SfxVolume = settingsData.SfxVolume;
    }

    public float SfxVolume
    {
        get => settingsData.SfxVolume;
        set
        {
            settingsData.SfxVolume = value;
            settingsData.Clamp();

            audioService.SfxVolume = settingsData.SfxVolume;
            settingsRepository.Save(settingsData);
        }
    }
}
