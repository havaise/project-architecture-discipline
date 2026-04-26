using ProjectArchitecture.Composition;

public static class GameServicesFactory
{
    public static GameServices CreateDefault()
    {
        ISaveService saveService = new JsonFileSaveService();
        ISettingsRepository settingsRepository = new SettingsRepository();
        IAudioService audioService = new UnityAudioService();
        ISceneLoader sceneLoader = new UnitySceneLoader();
        IGameSessionState gameSessionState = new GameSessionState();
        ISaveGameRepository saveGameRepository = new SaveGameRepository(saveService);

        SettingsData settings = settingsRepository.Load();
        audioService.SfxVolume = settings.SfxVolume;

        ServiceLocator locator = new ServiceLocator();
        locator.Register<ISaveService>(saveService);
        locator.Register<ISettingsRepository>(settingsRepository);
        locator.Register<IAudioService>(audioService);
        locator.Register<ISaveGameRepository>(saveGameRepository);
        locator.Register<ISceneLoader>(sceneLoader);
        locator.Register<IGameSessionState>(gameSessionState);

        return new GameServices(locator);
    }
}
