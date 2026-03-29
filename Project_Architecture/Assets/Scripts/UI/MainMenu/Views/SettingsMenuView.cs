using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Button backButton;

    private bool suppressEvents;

    public event Action<float> MusicVolumeChanged;
    public event Action BackClicked;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }
    }

    private void OnEnable()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    private void OnDisable()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
        }
    }

    public void SetMusicVolume(float value)
    {
        if (musicVolumeSlider == null)
        {
            return;
        }

        suppressEvents = true;
        musicVolumeSlider.SetValueWithoutNotify(value);
        suppressEvents = false;
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

    private void OnMusicVolumeChanged(float value)
    {
        if (suppressEvents)
        {
            return;
        }

        MusicVolumeChanged?.Invoke(value);
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}
