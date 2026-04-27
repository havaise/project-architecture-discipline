using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuView : PanelViewBase
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;

    public event Action MainMenuClicked;
    public event Action SaveClicked;
    public event Action LoadClicked;

    private void OnEnable()
    {
        UiBindingUtility.Add(mainMenuButton, OnMainMenuClicked);
        UiBindingUtility.Add(saveButton, OnSaveClicked);
        UiBindingUtility.Add(loadButton, OnLoadClicked);
    }

    private void OnDisable()
    {
        UiBindingUtility.Remove(mainMenuButton, OnMainMenuClicked);
        UiBindingUtility.Remove(saveButton, OnSaveClicked);
        UiBindingUtility.Remove(loadButton, OnLoadClicked);
    }

    private void OnMainMenuClicked()
    {
        MainMenuClicked?.Invoke();
    }

    private void OnSaveClicked()
    {
        SaveClicked?.Invoke();
    }

    private void OnLoadClicked()
    {
        LoadClicked?.Invoke();
    }
}
