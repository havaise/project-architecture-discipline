using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HudView : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private MonoBehaviour healthSource;
    [SerializeField] private MonoBehaviour cooldownSource;

    [Header("Widgets")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text cooldownText;

    [Header("Auto Resolve")]
    [SerializeField] private bool autoFindInParent = true;
    [SerializeField] private bool autoFindInScene = true;

    private HudController controller;

    private void Awake()
    {
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

        controller?.Tick();
    }

    public void SetSources(MonoBehaviour health, MonoBehaviour cooldown)
    {
        healthSource = health;
        cooldownSource = cooldown;

        controller?.Dispose();
        controller = null;
        TryCreateController();
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

    private void TryCreateController()
    {
        if (controller != null)
        {
            return;
        }

        IHealth health = ResolveHealth();
        IMagicCooldownProvider cooldown = ResolveCooldownProvider();
        if (health == null && cooldown == null)
        {
            return;
        }

        HudModel model = new HudModel(health, cooldown);
        controller = new HudController(model, this);
    }

    private IHealth ResolveHealth()
    {
        IHealth resolved = healthSource as IHealth;
        if (resolved != null)
        {
            return resolved;
        }

        if (autoFindInParent)
        {
            resolved = GetComponentInParent<IHealth>();
            if (resolved != null)
            {
                healthSource = resolved as MonoBehaviour;
                return resolved;
            }
        }

        if (!autoFindInScene)
        {
            return null;
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            HealthComponent playerHealth = taggedPlayer.GetComponentInChildren<HealthComponent>();
            if (playerHealth != null)
            {
                healthSource = playerHealth;
                return playerHealth;
            }
        }

        return null;
    }

    private IMagicCooldownProvider ResolveCooldownProvider()
    {
        IMagicCooldownProvider resolved = cooldownSource as IMagicCooldownProvider;
        if (resolved != null)
        {
            return resolved;
        }

        if (autoFindInParent)
        {
            resolved = GetComponentInParent<IMagicCooldownProvider>();
            if (resolved != null)
            {
                cooldownSource = resolved as MonoBehaviour;
                return resolved;
            }
        }

        if (!autoFindInScene)
        {
            return null;
        }

        PlayerCombatSystem combatSystem = FindFirstObjectByType<PlayerCombatSystem>();
        if (combatSystem == null)
        {
            return null;
        }

        cooldownSource = combatSystem;
        return combatSystem;
    }
}
