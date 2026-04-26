using UnityEngine;

using ServiceLocator = ProjectArchitecture.Composition.ServiceLocator;

[DefaultExecutionOrder(-1000)]
public class GameEntryPoint : MonoBehaviour
{
    public static GameServices Services { get; private set; }

    [SerializeField] private bool keepAliveBetweenScenes = true;

    private static GameEntryPoint instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (keepAliveBetweenScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (Services != null)
        {
            return;
        }

        ISaveService saveService = new JsonFileSaveService();
        ISettingsRepository settingsRepository = new SettingsRepository();
        IAudioService audioService = new UnityAudioService();
        ISceneLoader sceneLoader = new UnitySceneLoader();
        IGameSessionState gameSessionState = new GameSessionState();

        SettingsData settings = settingsRepository.Load();
        audioService.SfxVolume = settings.SfxVolume;

        ServiceLocator locator = new ServiceLocator();
        locator.Register<ISaveService>(saveService);
        locator.Register<ISettingsRepository>(settingsRepository);
        locator.Register<IAudioService>(audioService);
        locator.Register<ISaveGameRepository>(new SaveGameRepository(saveService));
        locator.Register<ISceneLoader>(sceneLoader);
        locator.Register<IGameSessionState>(gameSessionState);

        // Keep this global access point narrow: composition code resolves interfaces here,
        // while gameplay rules still receive dependencies explicitly through constructors.
        Services = new GameServices(locator);
    }

    private void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        Services?.Locator.Clear();
        Services = null;
        instance = null;
    }
}
