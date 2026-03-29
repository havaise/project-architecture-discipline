using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PauseMenuController : IDisposable
{
    private readonly PauseMenuView view;
    private readonly IInputService inputService;
    private readonly ISaveService saveService;
    private readonly ISceneLoader sceneLoader;
    private readonly IGameSessionState gameSessionState;
    private readonly Transform playerTransform;
    private readonly MonoBehaviour[] gameplayComponentsToToggle;
    private readonly string mainMenuSceneName;

    private bool isPaused;

    public PauseMenuController(
        PauseMenuView view,
        IInputService inputService,
        ISaveService saveService,
        ISceneLoader sceneLoader,
        IGameSessionState gameSessionState,
        Transform playerTransform,
        MonoBehaviour[] gameplayComponentsToToggle,
        string mainMenuSceneName)
    {
        this.view = view ?? throw new ArgumentNullException(nameof(view));
        this.inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        this.saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        this.sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
        this.gameSessionState = gameSessionState ?? throw new ArgumentNullException(nameof(gameSessionState));
        this.playerTransform = playerTransform;
        this.gameplayComponentsToToggle = gameplayComponentsToToggle;
        this.mainMenuSceneName = string.IsNullOrWhiteSpace(mainMenuSceneName) ? "MaIn" : mainMenuSceneName;
    }

    public void Initialize()
    {
        view.MainMenuClicked += OnMainMenuClicked;
        view.SaveClicked += OnSaveClicked;
        view.LoadClicked += OnLoadClicked;
        inputService.PauseStarted += OnPauseStarted;
        view.Hide();
    }

    public void Dispose()
    {
        view.MainMenuClicked -= OnMainMenuClicked;
        view.SaveClicked -= OnSaveClicked;
        view.LoadClicked -= OnLoadClicked;
        inputService.PauseStarted -= OnPauseStarted;
        ResumeIfPaused();
    }

    private void OnPauseStarted()
    {
        if (isPaused)
        {
            Resume();
            return;
        }

        Pause();
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        SetGameplayComponentsEnabled(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        view.Show();
    }

    private void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetGameplayComponentsEnabled(true);
        view.Hide();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ResumeIfPaused()
    {
        if (isPaused)
        {
            Resume();
        }
    }

    private void SetGameplayComponentsEnabled(bool enabled)
    {
        if (gameplayComponentsToToggle == null)
        {
            return;
        }

        for (int i = 0; i < gameplayComponentsToToggle.Length; i++)
        {
            MonoBehaviour component = gameplayComponentsToToggle[i];
            if (component != null)
            {
                component.enabled = enabled;
            }
        }
    }

    private void OnMainMenuClicked()
    {
        ResumeIfPaused();
        sceneLoader.LoadScene(mainMenuSceneName);
    }

    private void OnSaveClicked()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("PauseMenuController: player transform is missing, save skipped.");
            return;
        }

        var data = new SaveGameData
        {
            SceneName = SceneManager.GetActiveScene().name,
            PlayerPosition = playerTransform.position,
            PlayerRotation = playerTransform.rotation
        };

        saveService.Save(data);
    }

    private void OnLoadClicked()
    {
        if (!saveService.TryLoad(out SaveGameData loadedData))
        {
            Debug.LogWarning("PauseMenuController: save file not found.");
            return;
        }

        string activeSceneName = SceneManager.GetActiveScene().name;
        if (string.Equals(activeSceneName, loadedData.SceneName, StringComparison.Ordinal))
        {
            if (playerTransform != null)
            {
                playerTransform.SetPositionAndRotation(loadedData.PlayerPosition, loadedData.PlayerRotation);
            }

            ResumeIfPaused();
            return;
        }

        gameSessionState.SetPendingLoadedGame(loadedData);
        ResumeIfPaused();
        sceneLoader.LoadScene(loadedData.SceneName);
    }
}
