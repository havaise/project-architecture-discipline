using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuView : PanelViewBase
{
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button backButton;

    private bool suppressEvents;

    public event Action<float> SfxVolumeChanged;
    public event Action BackClicked;

    private void OnEnable()
    {
        UiBindingUtility.Add(sfxVolumeSlider, OnSfxVolumeChanged);
        UiBindingUtility.Add(backButton, OnBackClicked);
    }

    private void OnDisable()
    {
        UiBindingUtility.Remove(sfxVolumeSlider, OnSfxVolumeChanged);
        UiBindingUtility.Remove(backButton, OnBackClicked);
    }

    public void SetSfxVolume(float value)
    {
        if (sfxVolumeSlider == null)
        {
            return;
        }

        suppressEvents = true;
        sfxVolumeSlider.SetValueWithoutNotify(value);
        suppressEvents = false;
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (suppressEvents)
        {
            return;
        }

        SfxVolumeChanged?.Invoke(value);
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}
