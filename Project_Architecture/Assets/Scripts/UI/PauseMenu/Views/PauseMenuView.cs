using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;

    public event Action MainMenuClicked;
    public event Action SaveClicked;
    public event Action LoadClicked;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }
    }

    private void OnEnable()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveClicked);
        }

        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadClicked);
        }
    }

    private void OnDisable()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(OnSaveClicked);
        }

        if (loadButton != null)
        {
            loadButton.onClick.RemoveListener(OnLoadClicked);
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
