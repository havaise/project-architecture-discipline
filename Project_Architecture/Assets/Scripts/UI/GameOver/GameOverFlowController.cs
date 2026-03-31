using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameOverFlowController
{
    private readonly GameOverModel model;
    private readonly GameOverView view;
    private readonly IHealth playerHealth;
    private readonly IInputService inputService;
    private readonly MonoBehaviour[] componentsToDisable;
    private readonly bool pauseTimeOnGameOver;
    private readonly bool unlockCursorOnGameOver;

    private bool initialized;

    public GameOverFlowController(
        GameOverModel model,
        GameOverView view,
        IHealth playerHealth,
        IInputService inputService,
        MonoBehaviour[] componentsToDisable,
        bool pauseTimeOnGameOver,
        bool unlockCursorOnGameOver)
    {
        this.model = model;
        this.view = view;
        this.playerHealth = playerHealth;
        this.inputService = inputService;
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
        if (!model.TryTrigger())
        {
            return;
        }

        inputService?.DisableGameplay();
        DisableControls();

        if (pauseTimeOnGameOver)
        {
            Time.timeScale = 0f;
        }

        view.Show();

        if (unlockCursorOnGameOver)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnRestartRequested()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
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
