using System;

public sealed class SettingsMenuController : IDisposable
{
    private readonly SettingsMenuView view;
    private readonly SettingsMenuModel model;

    public event Action BackRequested;

    public SettingsMenuController(SettingsMenuView view, SettingsMenuModel model)
    {
        this.view = view;
        this.model = model;
    }

    public void Initialize()
    {
        view.MusicVolumeChanged += OnMusicVolumeChanged;
        view.BackClicked += OnBackClicked;
        view.SetMusicVolume(model.MusicVolume);
        view.Hide();
    }

    public void Show()
    {
        view.SetMusicVolume(model.MusicVolume);
        view.Show();
    }

    public void Hide()
    {
        view.Hide();
    }

    public void Dispose()
    {
        view.MusicVolumeChanged -= OnMusicVolumeChanged;
        view.BackClicked -= OnBackClicked;
    }

    private void OnMusicVolumeChanged(float value)
    {
        model.MusicVolume = value;
    }

    private void OnBackClicked()
    {
        BackRequested?.Invoke();
    }
}
