using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : PanelViewBase
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button settingsButton;

    public event Action PlayClicked;
    public event Action LoadClicked;
    public event Action SettingsClicked;

    private void OnEnable()
    {
        UiBindingUtility.Add(playButton, OnPlayClicked);
        UiBindingUtility.Add(loadButton, OnLoadClicked);
        UiBindingUtility.Add(settingsButton, OnSettingsClicked);
    }

    private void OnDisable()
    {
        UiBindingUtility.Remove(playButton, OnPlayClicked);
        UiBindingUtility.Remove(loadButton, OnLoadClicked);
        UiBindingUtility.Remove(settingsButton, OnSettingsClicked);
    }

    private void OnPlayClicked()
    {
        PlayClicked?.Invoke();
    }

    private void OnLoadClicked()
    {
        LoadClicked?.Invoke();
    }

    private void OnSettingsClicked()
    {
        SettingsClicked?.Invoke();
    }
}
