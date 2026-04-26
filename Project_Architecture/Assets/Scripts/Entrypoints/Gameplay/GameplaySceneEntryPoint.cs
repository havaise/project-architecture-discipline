using UnityEngine;

using IServiceLocator = ProjectArchitecture.Composition.IServiceLocator;

public class GameplaySceneEntryPoint : MonoBehaviour
{
    [Header("Scene Services")]
    [SerializeField] private MonoBehaviour inputServiceSource;
    [SerializeField] private string mainMenuSceneName = "MaInMenu";

    [Header("Scene Components")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombatSystem playerCombatSystem;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerView playerView;
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

        RegisterSceneServices();
        ResolveSceneComponents();
        ResolvePlayerTransform();
        InitializePlayerMvc();
        ConfigureHud();
        InitializeGameSaveInteractor();
        gameSaveInteractor?.ApplyPendingLoadedGame();
        InitializePauseMenu();
    }

    private void OnDestroy()
    {
        pauseMenuController?.Dispose();

        if (GameEntryPoint.Services != null)
        {
            IServiceLocator services = GameEntryPoint.Services.Locator;

            if (services.TryGet<IInputService>(out IInputService registeredInputService)
                && ReferenceEquals(registeredInputService, inputService))
            {
                services.Remove<IInputService>();
            }

            if (services.TryGet<IGameSaveInteractor>(out IGameSaveInteractor registeredGameSaveInteractor)
                && ReferenceEquals(registeredGameSaveInteractor, gameSaveInteractor))
            {
                services.Remove<IGameSaveInteractor>();
            }
        }
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

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        if (playerView == null)
        {
            playerView = FindFirstObjectByType<PlayerView>();
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

    private void RegisterSceneServices()
    {
        IServiceLocator services = GameEntryPoint.Services.Locator;

        if (!services.TryRegister<IInputService>(inputService))
        {
            Debug.LogWarning("GameplaySceneEntryPoint: IInputService is already registered for this scene.", this);
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

    private void InitializePlayerMvc()
    {
        if (playerTransform == null)
        {
            return;
        }

        if (playerView == null)
        {
            playerView = playerTransform.GetComponent<PlayerView>();
            if (playerView == null)
            {
                playerView = playerTransform.gameObject.AddComponent<PlayerView>();
            }
        }

        if (playerController == null)
        {
            playerController = playerTransform.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = playerTransform.gameObject.AddComponent<PlayerController>();
            }
        }

        playerController.Initialize(inputService);
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

        if (!GameEntryPoint.Services.Locator.TryRegister<IGameSaveInteractor>(gameSaveInteractor))
        {
            Debug.LogWarning("GameplaySceneEntryPoint: IGameSaveInteractor is already registered for this scene.", this);
        }
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
                playerController,
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
