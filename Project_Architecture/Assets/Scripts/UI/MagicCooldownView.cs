using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MagicCooldownView : MonoBehaviour
{
    [Header("References")]
    [FormerlySerializedAs("playerCombatSystem")]
    [SerializeField] private MonoBehaviour cooldownProviderSource;
    [SerializeField] private TMP_Text cooldownText;

    private IMagicCooldownProvider cooldownProvider;

    private void Awake()
    {
        ResolveCooldownProvider();
    }

    private void Update()
    {
        if (cooldownText == null || !ResolveCooldownProvider())
        {
            return;
        }

        float cooldownRemaining = cooldownProvider.MagicCooldownRemaining;
        cooldownText.text = cooldownRemaining > 0f ? Mathf.CeilToInt(cooldownRemaining).ToString() : string.Empty;
    }

    private bool ResolveCooldownProvider()
    {
        if (cooldownProvider != null)
        {
            return true;
        }

        cooldownProvider = cooldownProviderSource as IMagicCooldownProvider;
        if (cooldownProvider == null)
        {
            PlayerCombatSystem combatSystem = FindFirstObjectByType<PlayerCombatSystem>();
            if (combatSystem != null)
            {
                cooldownProviderSource = combatSystem;
                cooldownProvider = combatSystem;
            }
        }

        return cooldownProvider != null;
    }
}


