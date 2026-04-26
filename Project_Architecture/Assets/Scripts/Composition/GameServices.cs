using ProjectArchitecture.Composition;

public sealed class GameServices
{
    private readonly IServiceLocator locator;

    public GameServices(IServiceLocator locator)
    {
        this.locator = locator ?? throw new System.ArgumentNullException(nameof(locator));
    }

    public GameServices(
        IAudioService audioService,
        ISettingsRepository settingsRepository,
        ISaveGameRepository saveGameRepository,
        ISceneLoader sceneLoader,
        IGameSessionState gameSessionState)
        : this(CreateDefaultLocator(
            audioService,
            settingsRepository,
            saveGameRepository,
            sceneLoader,
            gameSessionState))
    {
    }

    public IServiceLocator Locator => locator;
    public IAudioService AudioService => locator.Get<IAudioService>();
    public ISettingsRepository SettingsRepository => locator.Get<ISettingsRepository>();
    public ISaveGameRepository SaveGameRepository => locator.Get<ISaveGameRepository>();
    public ISceneLoader SceneLoader => locator.Get<ISceneLoader>();
    public IGameSessionState GameSessionState => locator.Get<IGameSessionState>();

    public TService Get<TService>() where TService : class
    {
        return locator.Get<TService>();
    }

    public bool TryGet<TService>(out TService service) where TService : class
    {
        return locator.TryGet(out service);
    }

    public bool Contains<TService>() where TService : class
    {
        return locator.Contains<TService>();
    }

    private static IServiceLocator CreateDefaultLocator(
        IAudioService audioService,
        ISettingsRepository settingsRepository,
        ISaveGameRepository saveGameRepository,
        ISceneLoader sceneLoader,
        IGameSessionState gameSessionState)
    {
        ServiceLocator serviceLocator = new ServiceLocator();
        serviceLocator.Register<IAudioService>(audioService);
        serviceLocator.Register<ISettingsRepository>(settingsRepository);
        serviceLocator.Register<ISaveGameRepository>(saveGameRepository);
        serviceLocator.Register<ISceneLoader>(sceneLoader);
        serviceLocator.Register<IGameSessionState>(gameSessionState);
        return serviceLocator;
    }
}
