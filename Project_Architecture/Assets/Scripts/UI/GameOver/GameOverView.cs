using System;
using UnityEngine;
using UnityEngine.UI;

public class GameOverView : PanelViewBase
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject restartMenuRoot;

    public event Action RestartClicked;
    public event Action QuitClicked;

    protected override void Awake()
    {
        if (root == null && restartMenuRoot != null)
        {
            root = restartMenuRoot;
        }

        base.Awake();
        Hide();
    }

    private void OnEnable()
    {
        UiBindingUtility.Add(restartButton, OnRestartClicked);
        UiBindingUtility.Add(quitButton, OnQuitClicked);
    }

    private void OnDisable()
    {
        UiBindingUtility.Remove(restartButton, OnRestartClicked);
        UiBindingUtility.Remove(quitButton, OnQuitClicked);
    }

    private void OnRestartClicked()
    {
        RestartClicked?.Invoke();
    }

    private void OnQuitClicked()
    {
        QuitClicked?.Invoke();
    }
}
