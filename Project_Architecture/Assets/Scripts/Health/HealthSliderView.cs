using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Slider))]
public class HealthSliderView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthComponent targetHealth;
    [SerializeField] private Slider slider;
    [SerializeField] private bool autoFindHealthInParent = true;

    private void Awake()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }

        if (targetHealth == null && autoFindHealthInParent)
        {
            targetHealth = GetComponentInParent<HealthComponent>();
        }

        RefreshInstant();
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshInstant();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void SetTarget(HealthComponent healthComponent)
    {
        if (targetHealth == healthComponent)
        {
            return;
        }

        Unsubscribe();
        targetHealth = healthComponent;
        Subscribe();
        RefreshInstant();
    }

    private void Subscribe()
    {
        if (targetHealth == null)
        {
            return;
        }

        targetHealth.HealthChanged -= OnHealthChanged;
        targetHealth.HealthChanged += OnHealthChanged;
    }

    private void Unsubscribe()
    {
        if (targetHealth == null)
        {
            return;
        }

        targetHealth.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int current, int max)
    {
        UpdateSlider(current, max);
    }

    private void RefreshInstant()
    {
        if (slider == null || targetHealth == null)
        {
            return;
        }

        UpdateSlider(targetHealth._current, targetHealth._max);
    }

    private void UpdateSlider(int current, int max)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = Mathf.Max(1, max);
        slider.value = Mathf.Clamp(current, 0, slider.maxValue);
    }
}
