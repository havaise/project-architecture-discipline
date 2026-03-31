using TMPro;
using UnityEngine;

public class MagicCooldownView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonoBehaviour cooldownProviderSource;
    [SerializeField] private TMP_Text cooldownText;

    private MagicCooldownController controller;

    private void Awake()
    {
        TryCreateController();
    }

    private void Update()
    {
        if (controller == null)
        {
            TryCreateController();
        }

        controller?.Tick();
    }

    public void Render(float cooldownRemaining)
    {
        if (cooldownText == null)
        {
            return;
        }

        cooldownText.text = cooldownRemaining > 0f
            ? Mathf.CeilToInt(cooldownRemaining).ToString()
            : string.Empty;
    }

    public void SetCooldownProvider(MonoBehaviour providerSource)
    {
        cooldownProviderSource = providerSource;
        controller = null;
        TryCreateController();
    }

    private void TryCreateController()
    {
        if (controller != null)
        {
            return;
        }

        IMagicCooldownProvider provider = ResolveCooldownProvider();
        if (provider == null)
        {
            return;
        }

        MagicCooldownModel model = new MagicCooldownModel(provider);
        controller = new MagicCooldownController(model, this);
    }

    private IMagicCooldownProvider ResolveCooldownProvider()
    {
        IMagicCooldownProvider provider = cooldownProviderSource as IMagicCooldownProvider;
        if (provider != null)
        {
            return provider;
        }

        PlayerCombatSystem combatSystem = FindFirstObjectByType<PlayerCombatSystem>();
        if (combatSystem == null)
        {
            return null;
        }

        cooldownProviderSource = combatSystem;
        return combatSystem;
    }
}

