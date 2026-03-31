using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplaySceneEntryPoint : MonoBehaviour
{
    [Header("Scene Services")]
    [SerializeField] private MonoBehaviour inputServiceSource;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Scene Components")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombatSystem playerCombatSystem;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private GameOverController gameOverController;
    [SerializeField] private HudView hudView;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PauseMenuView pauseMenuView;
    [SerializeField] private MonoBehaviour[] gameplayComponentsToToggle;

    private IInputService inputService;
    private PauseMenuController pauseMenuController;

    private void Awake()
    {
        if (GameEntryPoint.Services == null)
        {
            Debug.LogError("GameplaySceneEntryPoint: GameEntryPoint is not initialized. Add GameEntryPoint to bootstrap scene.", this);
            return;
        }

        if (!InputServiceResolver.TryResolve(ref inputService, ref inputServiceSource))
        {
            Debug.LogError("GameplaySceneEntryPoint: InputService not found.", this);
            return;
        }

        ResolveSceneComponents();
        ResolvePlayerTransform();
        ConfigureHud();
        ApplyPendingLoadedGame();
        InitializePauseMenu();
    }

    private void OnDestroy()
    {
        pauseMenuController?.Dispose();
    }

    private void ResolveSceneComponents()
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
        
        if (hudView == null)
        {
            hudView = FindFirstObjectByType<HudView>();
        }
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

    private void ConfigureHud()
    {
        if (hudView != null)
        {
            HealthComponent playerHealth = ResolvePlayerHealthComponent();
            hudView.SetSources(playerHealth, playerCombatSystem);
        }
    }

    private HealthComponent ResolvePlayerHealthComponent()
    {
        if (playerTransform != null)
        {
            HealthComponent healthFromTransform = playerTransform.GetComponentInChildren<HealthComponent>();
            if (healthFromTransform != null)
            {
                return healthFromTransform;
            }
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            HealthComponent healthFromTag = taggedPlayer.GetComponentInChildren<HealthComponent>();
            if (healthFromTag != null)
            {
                return healthFromTag;
            }
        }

        if (playerMovement != null)
        {
            return playerMovement.GetComponentInChildren<HealthComponent>();
        }

        return null;
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
}

