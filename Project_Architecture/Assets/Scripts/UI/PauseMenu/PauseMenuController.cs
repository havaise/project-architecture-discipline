using System;
using UnityEngine;

public sealed class PauseMenuController : IDisposable
{
    private readonly PauseMenuView view;
    private readonly IInputService inputService;
    private readonly IGameSaveInteractor gameSaveInteractor;
    private readonly ISceneLoader sceneLoader;
    private readonly MonoBehaviour[] gameplayComponentsToToggle;
    private readonly string mainMenuSceneName;
    private bool isPaused;
    private int lastPauseToggleFrame = -1;

    public PauseMenuController(
        PauseMenuView view,
        IInputService inputService,
        IGameSaveInteractor gameSaveInteractor,
        ISceneLoader sceneLoader,
        MonoBehaviour[] gameplayComponentsToToggle,
        string mainMenuSceneName)
    {
        this.view = view ?? throw new ArgumentNullException(nameof(view));
        this.inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
        this.gameSaveInteractor = gameSaveInteractor ?? throw new ArgumentNullException(nameof(gameSaveInteractor));
        this.sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
        this.gameplayComponentsToToggle = gameplayComponentsToToggle;
        this.mainMenuSceneName = string.IsNullOrWhiteSpace(mainMenuSceneName) ? "MainMenu" : mainMenuSceneName;
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

    public void Tick()
    {
        if (inputService.IsPausePressed())
        {
            HandlePauseToggleRequested();
        }
    }

    private void OnPauseStarted()
    {
        HandlePauseToggleRequested();
    }

    private void HandlePauseToggleRequested()
    {
        if (lastPauseToggleFrame == Time.frameCount)
        {
            return;
        }

        lastPauseToggleFrame = Time.frameCount;

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
        if (!gameSaveInteractor.SaveCurrentGame())
        {
            Debug.LogWarning("PauseMenuController: unable to save current game state.");
        }
    }

    private void OnLoadClicked()
    {
        if (!gameSaveInteractor.LoadGame())
        {
            Debug.LogWarning("PauseMenuController: save file not found.");
            return;
        }

        ResumeIfPaused();
    }
}
