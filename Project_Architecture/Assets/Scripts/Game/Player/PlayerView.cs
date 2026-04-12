using UnityEngine;

[DisallowMultipleComponent]
public class PlayerView : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombatSystem playerCombatSystem;

    private void Awake()
    {
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (playerCombatSystem == null)
        {
            playerCombatSystem = GetComponent<PlayerCombatSystem>();
        }
    }

    public void Apply(PlayerModel model)
    {
        if (model == null)
        {
            return;
        }

        if (playerMovement != null && playerMovement.enabled)
        {
            playerMovement.ProcessFrame(model);
        }

        if (playerCombatSystem != null && playerCombatSystem.enabled)
        {
            playerCombatSystem.ProcessFrame(model);
        }
    }
}
