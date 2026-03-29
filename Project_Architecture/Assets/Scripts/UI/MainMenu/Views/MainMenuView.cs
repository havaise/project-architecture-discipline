using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;

    public event Action PlayClicked;
    public event Action SettingsClicked;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }
    }

    private void OnEnable()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }
    }

    private void OnDisable()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }
    }

    public void Show()
    {
        if (root != null)
        {
            root.SetActive(true);
        }
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
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
