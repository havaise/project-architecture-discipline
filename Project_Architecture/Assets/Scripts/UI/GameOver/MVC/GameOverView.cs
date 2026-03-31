using System;
using UnityEngine;
using UnityEngine.UI;

public class GameOverView : MonoBehaviour
{
    [SerializeField] private GameObject restartMenuRoot;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    public event Action RestartClicked;
    public event Action QuitClicked;

    private void Awake()
    {
        Hide();
    }

    private void OnEnable()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void OnDisable()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitClicked);
        }
    }

    public void Show()
    {
        if (restartMenuRoot != null)
        {
            restartMenuRoot.SetActive(true);
        }
    }

    public void Hide()
    {
        if (restartMenuRoot != null)
        {
            restartMenuRoot.SetActive(false);
        }
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
