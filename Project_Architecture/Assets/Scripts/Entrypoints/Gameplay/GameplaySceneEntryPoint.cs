using UnityEngine;

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
    private IGameSaveInteractor gameSaveInteractor;

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
        InitializeGameSaveInteractor();
        gameSaveInteractor?.ApplyPendingLoadedGame();
        InitializePauseMenu();
    }

    private void OnDestroy()
    {
        pauseMenuController?.Dispose();
    }

    private void Update()
    {
        pauseMenuController?.Tick();
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

    private void InitializeGameSaveInteractor()
    {
        HealthComponent playerHealth = ResolvePlayerHealthComponent();
        ManaComponent playerMana = ResolvePlayerManaComponent();
        PlayerStatsComponent playerStats = ResolvePlayerStatsComponent();
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        IPlayerStateRepository playerRepository = new PlayerStateRepository(
            playerTransform,
            playerHealth,
            playerMana,
            playerStats);

        IEnemyStateRepository enemyRepository = new EnemyStateRepository(enemies);

        gameSaveInteractor = new GameSaveInteractor(
            GameEntryPoint.Services.SaveGameRepository,
            playerRepository,
            enemyRepository,
            GameEntryPoint.Services.GameSessionState,
            GameEntryPoint.Services.SceneLoader);
    }

    private void ConfigureHud()
    {
        if (hudView == null)
        {
            return;
        }

        HealthComponent playerHealth = ResolvePlayerHealthComponent();
        hudView.SetSources(playerHealth, playerCombatSystem);
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

        return playerMovement != null ? playerMovement.GetComponentInChildren<HealthComponent>() : null;
    }

    private ManaComponent ResolvePlayerManaComponent()
    {
        if (playerTransform != null)
        {
            ManaComponent manaFromTransform = playerTransform.GetComponentInChildren<ManaComponent>();
            if (manaFromTransform != null)
            {
                return manaFromTransform;
            }
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            ManaComponent manaFromTag = taggedPlayer.GetComponentInChildren<ManaComponent>();
            if (manaFromTag != null)
            {
                return manaFromTag;
            }
        }

        return playerMovement != null ? playerMovement.GetComponentInChildren<ManaComponent>() : null;
    }

    private PlayerStatsComponent ResolvePlayerStatsComponent()
    {
        if (playerTransform != null)
        {
            PlayerStatsComponent statsFromTransform = playerTransform.GetComponentInChildren<PlayerStatsComponent>();
            if (statsFromTransform != null)
            {
                return statsFromTransform;
            }
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            PlayerStatsComponent statsFromTag = taggedPlayer.GetComponentInChildren<PlayerStatsComponent>();
            if (statsFromTag != null)
            {
                return statsFromTag;
            }
        }

        return playerMovement != null ? playerMovement.GetComponentInChildren<PlayerStatsComponent>() : null;
    }

    private void InitializePauseMenu()
    {
        if (pauseMenuView == null)
        {
            Debug.LogError("GameplaySceneEntryPoint: PauseMenuView is not assigned.", this);
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
            gameSaveInteractor,
            GameEntryPoint.Services.SceneLoader,
            componentsToToggle,
            mainMenuSceneName);

        pauseMenuController.Initialize();
    }
}
