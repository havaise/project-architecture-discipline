using UnityEngine;

public sealed class GameOverFlowController
{
    private readonly GameOverView view;
    private readonly IHealth playerHealth;
    private readonly IInputService inputService;
    private readonly IGamePauseService gamePauseService;
    private readonly ICursorService cursorService;
    private readonly ISceneRestartService sceneRestartService;
    private readonly MonoBehaviour[] componentsToDisable;
    private readonly bool pauseTimeOnGameOver;
    private readonly bool unlockCursorOnGameOver;

    private bool initialized;
    private bool isTriggered;

    public GameOverFlowController(
        GameOverView view,
        IHealth playerHealth,
        IInputService inputService,
        IGamePauseService gamePauseService,
        ICursorService cursorService,
        ISceneRestartService sceneRestartService,
        MonoBehaviour[] componentsToDisable,
        bool pauseTimeOnGameOver,
        bool unlockCursorOnGameOver)
    {
        this.view = view;
        this.playerHealth = playerHealth;
        this.inputService = inputService;
        this.gamePauseService = gamePauseService;
        this.cursorService = cursorService;
        this.sceneRestartService = sceneRestartService;
        this.componentsToDisable = componentsToDisable;
        this.pauseTimeOnGameOver = pauseTimeOnGameOver;
        this.unlockCursorOnGameOver = unlockCursorOnGameOver;
    }

    public void Initialize()
    {
        if (initialized || playerHealth == null || view == null)
        {
            return;
        }

        playerHealth.Died += OnPlayerDied;
        view.RestartClicked += OnRestartRequested;
        view.QuitClicked += OnQuitRequested;
        initialized = true;
    }

    public void Dispose()
    {
        if (!initialized)
        {
            return;
        }

        playerHealth.Died -= OnPlayerDied;
        view.RestartClicked -= OnRestartRequested;
        view.QuitClicked -= OnQuitRequested;
        initialized = false;
    }

    private void OnPlayerDied()
    {
        if (isTriggered)
        {
            return;
        }

        isTriggered = true;
        inputService?.DisableGameplay();
        DisableControls();

        if (pauseTimeOnGameOver)
        {
            gamePauseService?.Pause();
        }

        view.Show();

        if (unlockCursorOnGameOver)
        {
            cursorService?.Show();
        }
    }

    private void OnRestartRequested()
    {
        gamePauseService?.Resume();
        sceneRestartService?.RestartActiveScene();
    }

    private void OnQuitRequested()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void DisableControls()
    {
        if (componentsToDisable == null)
        {
            return;
        }

        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            MonoBehaviour component = componentsToDisable[i];
            if (component != null)
            {
                component.enabled = false;
            }
        }
    }
}
