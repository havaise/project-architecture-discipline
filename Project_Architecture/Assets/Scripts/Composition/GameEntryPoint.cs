using UnityEngine;

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

        Services = GameServicesFactory.CreateDefault();
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
