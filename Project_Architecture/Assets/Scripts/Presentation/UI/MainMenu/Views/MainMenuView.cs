using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : PanelViewBase
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;

    public event Action PlayClicked;
    public event Action SettingsClicked;

    private void OnEnable()
    {
        UiBindingUtility.Add(playButton, OnPlayClicked);
        UiBindingUtility.Add(settingsButton, OnSettingsClicked);
    }

    private void OnDisable()
    {
        UiBindingUtility.Remove(playButton, OnPlayClicked);
        UiBindingUtility.Remove(settingsButton, OnSettingsClicked);
    }

    private void OnPlayClicked()
    {
        PlayClicked?.Invoke();
    }

    private void OnSettingsClicked()
    {
        SettingsClicked?.Invoke();
    }
}
