using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HudView : MonoBehaviour
{
    [Header("Widgets")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text cooldownText;

    private HudController controller;

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
        controller?.Tick();
    }

    public void Initialize(IHealth health, IMagicCooldownProvider cooldown)
    {
        controller?.Dispose();
        HudModel model = new HudModel(health, cooldown);
        controller = new HudController(model, this);
        controller?.Initialize();
    }

    public void RenderHealth(int current, int max)
    {
        if (healthSlider == null)
        {
            return;
        }

        healthSlider.minValue = 0f;
        healthSlider.maxValue = Mathf.Max(1f, max);
        healthSlider.value = Mathf.Clamp(current, 0f, healthSlider.maxValue);
    }

    public void RenderCooldown(float cooldownRemaining)
    {
        if (cooldownText == null)
        {
            return;
        }

        cooldownText.text = cooldownRemaining > 0f
            ? Mathf.CeilToInt(cooldownRemaining).ToString()
            : string.Empty;
    }
}
