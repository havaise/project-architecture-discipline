using ProjectArchitecture.Composition;

public sealed class GameServices
{
    private readonly IServiceLocator locator;

    public GameServices(IServiceLocator locator)
    {
        this.locator = locator ?? throw new System.ArgumentNullException(nameof(locator));
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
}
