using System;

public sealed class MainMenuController : IDisposable
{
    private readonly MainMenuView mainMenuView;
    private readonly SettingsMenuController settingsMenuController;
    private readonly ISceneLoader sceneLoader;
    private readonly string gameplaySceneName;

    public MainMenuController(
        MainMenuView mainMenuView,
        SettingsMenuController settingsMenuController,
        ISceneLoader sceneLoader,
        string gameplaySceneName)
    {
        this.mainMenuView = mainMenuView;
        this.settingsMenuController = settingsMenuController;
        this.sceneLoader = sceneLoader;
        this.gameplaySceneName = gameplaySceneName;
    }

    public void Initialize()
    {
        mainMenuView.PlayClicked += OnPlayClicked;
        mainMenuView.SettingsClicked += OnSettingsClicked;
        settingsMenuController.BackRequested += OnSettingsBack;

        settingsMenuController.Initialize();
        mainMenuView.Show();
    }

    public void Dispose()
    {
        mainMenuView.PlayClicked -= OnPlayClicked;
        mainMenuView.SettingsClicked -= OnSettingsClicked;
        settingsMenuController.BackRequested -= OnSettingsBack;
        settingsMenuController.Dispose();
    }

    private void OnPlayClicked()
    {
        sceneLoader.LoadScene(gameplaySceneName);
    }

    private void OnSettingsClicked()
    {
        mainMenuView.Hide();
        settingsMenuController.Show();
    }

    private void OnSettingsBack()
    {
        settingsMenuController.Hide();
        mainMenuView.Show();
    }
}
