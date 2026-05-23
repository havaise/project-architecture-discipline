using UnityEngine;

public sealed class VictoryFlowController
{
    private readonly VictoryView view;
    private readonly GameplayEventDirector gameplayEventDirector;
    private readonly IInputService inputService;
    private readonly IGamePauseService gamePauseService;
    private readonly ICursorService cursorService;
    private readonly ISceneRestartService sceneRestartService;
    private readonly MonoBehaviour[] componentsToDisable;
    private readonly bool pauseTimeOnVictory;
    private readonly bool unlockCursorOnVictory;

    private bool initialized;
    private bool shown;

    public VictoryFlowController(
        VictoryView view,
        GameplayEventDirector gameplayEventDirector,
        IInputService inputService,
        IGamePauseService gamePauseService,
        ICursorService cursorService,
        ISceneRestartService sceneRestartService,
        MonoBehaviour[] componentsToDisable,
        bool pauseTimeOnVictory,
        bool unlockCursorOnVictory)
    {
        this.view = view;
        this.gameplayEventDirector = gameplayEventDirector;
        this.inputService = inputService;
        this.gamePauseService = gamePauseService;
        this.cursorService = cursorService;
        this.sceneRestartService = sceneRestartService;
        this.componentsToDisable = componentsToDisable;
        this.pauseTimeOnVictory = pauseTimeOnVictory;
        this.unlockCursorOnVictory = unlockCursorOnVictory;
    }

    public void Initialize()
    {
        if (initialized || view == null || gameplayEventDirector == null)
        {
            return;
        }

        gameplayEventDirector.VictoryReached += OnVictoryReached;
        view.ContinueClicked += OnContinueClicked;
        view.RestartClicked += OnRestartClicked;
        view.Hide();
        initialized = true;
    }

    public void Dispose()
    {
        if (!initialized)
        {
            return;
        }

        gameplayEventDirector.VictoryReached -= OnVictoryReached;
        view.ContinueClicked -= OnContinueClicked;
        view.RestartClicked -= OnRestartClicked;
        initialized = false;
    }

    private void OnVictoryReached()
    {
        if (shown)
        {
            return;
        }

        shown = true;
        inputService?.DisableGameplay();
        SetControlsEnabled(false);

        if (pauseTimeOnVictory)
        {
            gamePauseService?.Pause();
        }

        view.Show();

        if (unlockCursorOnVictory)
        {
            cursorService?.Show();
        }
    }

    private void OnContinueClicked()
    {
        if (!shown)
        {
            return;
        }

        shown = false;
        view.Hide();
        gamePauseService?.Resume();
        SetControlsEnabled(true);
        inputService?.EnableGameplay();
        cursorService?.HideAndLock();
    }

    private void OnRestartClicked()
    {
        gamePauseService?.Resume();
        sceneRestartService?.RestartActiveScene();
    }

    private void SetControlsEnabled(bool enabled)
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
                component.enabled = enabled;
            }
        }
    }
}
