using UnityEngine;

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

        SettingsData settings = settingsRepository.Load();
        audioService.SfxVolume = settings.SfxVolume;

        Services = new GameServices(
            audioService,
            settingsRepository,
            new SaveGameRepository(saveService),
            new UnitySceneLoader(),
            new GameSessionState());
    }
}
