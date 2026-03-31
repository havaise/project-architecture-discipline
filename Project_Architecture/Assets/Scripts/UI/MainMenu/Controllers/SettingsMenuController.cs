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
        view.SfxVolumeChanged += OnSfxVolumeChanged;
        view.BackClicked += OnBackClicked;
        view.SetSfxVolume(model.SfxVolume);
        view.Hide();
    }

    public void Show()
    {
        view.SetSfxVolume(model.SfxVolume);
        view.Show();
    }

    public void Hide()
    {
        view.Hide();
    }

    public void Dispose()
    {
        view.SfxVolumeChanged -= OnSfxVolumeChanged;
        view.BackClicked -= OnBackClicked;
    }

    private void OnSfxVolumeChanged(float value)
    {
        model.SfxVolume = value;
    }

    private void OnBackClicked()
    {
        BackRequested?.Invoke();
    }
}
