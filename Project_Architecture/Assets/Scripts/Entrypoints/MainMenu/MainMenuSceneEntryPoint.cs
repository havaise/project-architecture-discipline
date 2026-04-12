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
            Debug.LogError("MainMenuSceneEntryPoint: assign both views.", this);
            return;
        }

        if (GameEntryPoint.Services == null)
        {
            Debug.LogError("MainMenuSceneEntryPoint: GameEntryPoint is not initialized. Add GameEntryPoint to bootstrap scene.", this);
            return;
        }

        SettingsMenuModel settingsModel = new SettingsMenuModel(
            GameEntryPoint.Services.AudioService,
            GameEntryPoint.Services.SettingsRepository);
        SettingsMenuController settingsController = new SettingsMenuController(settingsMenuView, settingsModel);
        IGameSaveInteractor gameSaveInteractor = new GameSaveInteractor(
            GameEntryPoint.Services.SaveGameRepository,
            null,
            null,
            GameEntryPoint.Services.GameSessionState,
            GameEntryPoint.Services.SceneLoader);

        mainMenuController = new MainMenuController(
            mainMenuView,
            settingsController,
            gameSaveInteractor,
            GameEntryPoint.Services.SceneLoader,
            gameplaySceneName);

        mainMenuController.Initialize();
    }

    private void OnDestroy()
    {
        mainMenuController?.Dispose();
    }
}
