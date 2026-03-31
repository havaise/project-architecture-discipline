public sealed class GameServices
{
    public GameServices(
        IAudioService audioService,
        ISaveService saveService,
        ISceneLoader sceneLoader,
        IGameSessionState gameSessionState)
    {
        AudioService = audioService;
        SaveService = saveService;
        SceneLoader = sceneLoader;
        GameSessionState = gameSessionState;
    }

    public IAudioService AudioService { get; }
    public ISaveService SaveService { get; }
    public ISceneLoader SceneLoader { get; }
    public IGameSessionState GameSessionState { get; }
}
