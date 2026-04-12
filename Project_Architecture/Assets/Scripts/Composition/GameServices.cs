public sealed class GameServices
{
    public GameServices(
        IAudioService audioService,
        ISettingsRepository settingsRepository,
        ISaveGameRepository saveGameRepository,
        ISceneLoader sceneLoader,
        IGameSessionState gameSessionState)
    {
        AudioService = audioService;
        SettingsRepository = settingsRepository;
        SaveGameRepository = saveGameRepository;
        SceneLoader = sceneLoader;
        GameSessionState = gameSessionState;
    }

    public IAudioService AudioService { get; }
    public ISettingsRepository SettingsRepository { get; }
    public ISaveGameRepository SaveGameRepository { get; }
    public ISceneLoader SceneLoader { get; }
    public IGameSessionState GameSessionState { get; }
}
