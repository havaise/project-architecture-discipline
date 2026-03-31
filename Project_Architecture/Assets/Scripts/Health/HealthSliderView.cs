using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Slider))]
public class HealthSliderView : MonoBehaviour
{
    [Header("References")]
    [FormerlySerializedAs("targetHealth")]
    [SerializeField] private MonoBehaviour targetHealthSource;
    [SerializeField] private Slider slider;
    [SerializeField] private bool autoFindHealthInParent = true;

    private IHealth targetHealth;

    private void Awake()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }

        ResolveTargetHealth();

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

    public void SetTarget(MonoBehaviour healthSource)
    {
        IHealth health = healthSource as IHealth;
        if (targetHealth == health)
        {
            return;
        }

        Unsubscribe();
        targetHealthSource = healthSource;
        targetHealth = health;
        Subscribe();
        RefreshInstant();
    }

    public void SetTarget(HealthComponent healthComponent)
    {
        SetTarget((MonoBehaviour)healthComponent);
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
        if (slider == null || !ResolveTargetHealth())
        {
            return;
        }

        UpdateSlider(targetHealth.Current, targetHealth.Max);
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

    private bool ResolveTargetHealth()
    {
        if (targetHealth != null)
        {
            return true;
        }

        targetHealth = targetHealthSource as IHealth;
        if (targetHealth != null)
        {
            return true;
        }

        if (!autoFindHealthInParent)
        {
            return false;
        }

        targetHealth = GetComponentInParent<IHealth>();
        targetHealthSource = targetHealth as MonoBehaviour;
        return targetHealth != null;
    }
}


