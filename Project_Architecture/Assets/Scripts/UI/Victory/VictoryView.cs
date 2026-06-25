using System;
using UnityEngine;
using UnityEngine.UI;

public class VictoryView : PanelViewBase
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;

    public event Action ContinueClicked;
    public event Action RestartClicked;

    protected override void Awake()
    {
        base.Awake();
        Hide();
    }

    private void OnEnable()
    {
        UiBindingUtility.Add(continueButton, OnContinueClicked);
        UiBindingUtility.Add(restartButton, OnRestartClicked);
    }

    private void OnDisable()
    {
        UiBindingUtility.Remove(continueButton, OnContinueClicked);
        UiBindingUtility.Remove(restartButton, OnRestartClicked);
    }

    private void OnContinueClicked()
    {
        ContinueClicked?.Invoke();
    }

    private void OnRestartClicked()
    {
        RestartClicked?.Invoke();
    }
}
