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

        Services = new GameServices(
            new UnityAudioService(),
            new JsonFileSaveService(),
            new UnitySceneLoader(),
            new GameSessionState());
    }
}
