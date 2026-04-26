using UnityEngine;

[DisallowMultipleComponent]
public class PlayerView : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombatSystem playerCombatSystem;

    private PlayerModel playerModel;
    private IInputService inputService;
    private bool initialized;

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

        playerModel = new PlayerModel();
    }

    private void Update()
    {
        if (!initialized || inputService == null)
        {
            return;
        }

        playerModel.UpdateFromInput(inputService);
        Apply(playerModel);
    }

    public void Initialize(IInputService inputService)
    {
        this.inputService = inputService;
        initialized = inputService != null;
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
