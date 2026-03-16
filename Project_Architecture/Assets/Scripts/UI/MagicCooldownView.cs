using TMPro;
using UnityEngine;

public class MagicCooldownView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombatSystem playerCombatSystem;
    [SerializeField] private TMP_Text cooldownText;

    private void Awake()
    {
        if (playerCombatSystem == null)
        {
            playerCombatSystem = FindFirstObjectByType<PlayerCombatSystem>();
        }
    }

    private void Update()
    {
        if (playerCombatSystem == null || cooldownText == null)
        {
            return;
        }

        float cooldownRemaining = playerCombatSystem.MagicCooldownRemaining;
        cooldownText.text = cooldownRemaining > 0f ? Mathf.CeilToInt(cooldownRemaining).ToString() : string.Empty;
    }
}
