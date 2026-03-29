using UnityEngine;

public class MainMenuSceneEntryPoint : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "MainScene";
    [SerializeField] private MainMenuView mainMenuView;
    [SerializeField] private SettingsMenuView settingsMenuView;

    private MainMenuController mainMenuController;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;

        if (mainMenuView == null || settingsMenuView == null)
        {
            Debug.LogError("MainMenuSceneEntryPoint: assign both menu views.", this);
            return;
        }

        EnsureGameEntryPoint();

        if (GameEntryPoint.Services == null)
        {
            Debug.LogError("MainMenuSceneEntryPoint: GameEntryPoint not initialized.", this);
            return;
        }

        var settingsModel = new SettingsMenuModel(GameEntryPoint.Services.AudioService);
        var settingsController = new SettingsMenuController(settingsMenuView, settingsModel);

        mainMenuController = new MainMenuController(
            mainMenuView,
            settingsController,
            GameEntryPoint.Services.SceneLoader,
            gameplaySceneName);

        mainMenuController.Initialize();
    }

    private void OnDestroy()
    {
        mainMenuController?.Dispose();
    }

    private static void EnsureGameEntryPoint()
    {
        if (GameEntryPoint.Services != null)
        {
            return;
        }

        GameEntryPoint existing = FindFirstObjectByType<GameEntryPoint>();
        if (existing != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject("GameEntryPoint");
        bootstrap.AddComponent<GameEntryPoint>();
    }
}
