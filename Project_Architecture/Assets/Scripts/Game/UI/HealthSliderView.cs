using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Slider))]
public class HealthSliderView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonoBehaviour targetHealthSource;
    [SerializeField] private Slider slider;
    [SerializeField] private bool autoFindHealthInParent = true;

    private HealthSliderController controller;

    private void Awake()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }

        TryCreateController();
    }

    private void OnEnable()
    {
        controller?.Initialize();
    }

    private void OnDisable()
    {
        controller?.Dispose();
    }

    private void Update()
    {
        if (controller == null)
        {
            TryCreateController();
            controller?.Initialize();
        }
    }

    public void SetTarget(MonoBehaviour healthSource)
    {
        targetHealthSource = healthSource;
        controller?.Dispose();
        controller = null;
        TryCreateController();
        controller?.Initialize();
    }

    public void SetTarget(HealthComponent healthComponent)
    {
        SetTarget((MonoBehaviour)healthComponent);
    }

    public void Render(int current, int max)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = Mathf.Max(1, max);
        slider.value = Mathf.Clamp(current, 0, slider.maxValue);
    }

    private void TryCreateController()
    {
        if (controller != null)
        {
            return;
        }

        IHealth health = ResolveTargetHealth();
        if (health == null)
        {
            return;
        }

        HealthSliderModel model = new HealthSliderModel(health);
        controller = new HealthSliderController(model, this);
    }

    private IHealth ResolveTargetHealth()
    {
        IHealth health = targetHealthSource as IHealth;
        if (health != null)
        {
            return health;
        }

        if (!autoFindHealthInParent)
        {
            return null;
        }

        health = GetComponentInParent<IHealth>();
        targetHealthSource = health as MonoBehaviour;
        return health;
    }
}

