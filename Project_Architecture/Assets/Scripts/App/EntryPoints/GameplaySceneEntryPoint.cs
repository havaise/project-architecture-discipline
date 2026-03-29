using UnityEngine;

public class GameplaySceneEntryPoint : MonoBehaviour
{
    [Header("Scene Services")]
    [SerializeField] private InputService inputService;

    [Header("Scene Components")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombatSystem playerCombatSystem;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private ThirdPersonCamera thirdPersonCamera;
    [SerializeField] private GameOverController gameOverController;

    private void Awake()
    {
        if (GameEntryPoint.Services == null)
        {
            Debug.LogError("GameplaySceneEntryPoint: GameEntryPoint is not initialized.", this);
            return;
        }

        if (inputService == null)
        {
            inputService = FindFirstObjectByType<InputService>();
        }

        if (inputService == null)
        {
            Debug.LogError("GameplaySceneEntryPoint: InputService is not found.", this);
            return;
        }

        InjectInputService(inputService);
    }

    private void InjectInputService(IInputService service)
    {
        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }

        if (playerCombatSystem == null)
        {
            playerCombatSystem = FindFirstObjectByType<PlayerCombatSystem>();
        }

        if (playerAnimationController == null)
        {
            playerAnimationController = FindFirstObjectByType<PlayerAnimationController>();
        }

        if (thirdPersonCamera == null)
        {
            thirdPersonCamera = FindFirstObjectByType<ThirdPersonCamera>();
        }

        if (gameOverController == null)
        {
            gameOverController = FindFirstObjectByType<GameOverController>();
        }

        playerMovement?.SetInputService(service);
        playerCombatSystem?.SetInputService(service);
        playerAnimationController?.SetInputService(service);
        thirdPersonCamera?.SetInputService(service);
        gameOverController?.SetInputService(service);
    }
}
