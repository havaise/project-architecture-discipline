using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplaySceneEntryPoint : MonoBehaviour
{
    [Header("Scene Services")]
    [SerializeField] private InputService inputService;
    [SerializeField] private string mainMenuSceneName = "MaIn";

    [Header("Scene Components")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombatSystem playerCombatSystem;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private GameOverController gameOverController;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PauseMenuView pauseMenuView;
    [SerializeField] private MonoBehaviour[] gameplayComponentsToToggle;

    private PauseMenuController pauseMenuController;

    private void Awake()
    {
        EnsureGameEntryPoint();

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
        ResolvePlayerTransform();
        ApplyPendingLoadedGame();
        InitializePauseMenu();
    }

    private void OnDestroy()
    {
        pauseMenuController?.Dispose();
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

        if (gameOverController == null)
        {
            gameOverController = FindFirstObjectByType<GameOverController>();
        }

        playerMovement?.SetInputService(service);
        playerCombatSystem?.SetInputService(service);
        playerAnimationController?.SetInputService(service);
        gameOverController?.SetInputService(service);
    }

    private void ResolvePlayerTransform()
    {
        if (playerTransform != null)
        {
            return;
        }

        if (playerMovement != null)
        {
            playerTransform = playerMovement.transform;
            return;
        }

        PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
        if (movement != null)
        {
            playerTransform = movement.transform;
        }
    }

    private void ApplyPendingLoadedGame()
    {
        SaveGameData pending = GameEntryPoint.Services.GameSessionState.ConsumePendingLoadedGame();
        if (pending == null || playerTransform == null)
        {
            return;
        }

        string activeSceneName = SceneManager.GetActiveScene().name;
        if (!string.Equals(activeSceneName, pending.SceneName, System.StringComparison.Ordinal))
        {
            return;
        }

        playerTransform.SetPositionAndRotation(pending.PlayerPosition, pending.PlayerRotation);
    }

    private void InitializePauseMenu()
    {
        if (pauseMenuView == null)
        {
            return;
        }

        MonoBehaviour[] componentsToToggle = gameplayComponentsToToggle;
        if (componentsToToggle == null || componentsToToggle.Length == 0)
        {
            componentsToToggle = new MonoBehaviour[]
            {
                playerMovement,
                playerCombatSystem,
                playerAnimationController
            };
        }

        pauseMenuController = new PauseMenuController(
            pauseMenuView,
            inputService,
            GameEntryPoint.Services.SaveService,
            GameEntryPoint.Services.SceneLoader,
            GameEntryPoint.Services.GameSessionState,
            playerTransform,
            componentsToToggle,
            mainMenuSceneName);

        pauseMenuController.Initialize();
    }

    private static void EnsureGameEntryPoint()
    {
        if (GameEntryPoint.Services != null)
        {
            return;
        }

        GameEntryPoint existing = FindFirstObjectByType<GameEntryPoint>();
        if (existing != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject("GameEntryPoint");
        bootstrap.AddComponent<GameEntryPoint>();
    }
}
