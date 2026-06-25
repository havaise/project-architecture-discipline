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
    [SerializeField] private PlayerView playerView;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private GameOverController gameOverController;
    [SerializeField] private HudView hudView;
    [SerializeField] private VictoryView victoryView;
    [SerializeField] private GameplayEventDirector gameplayEventDirector;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PauseMenuView pauseMenuView;
    [SerializeField] private MonoBehaviour[] gameplayComponentsToToggle;

    private IInputService inputService;
    private PauseMenuController pauseMenuController;
    private VictoryFlowController victoryFlowController;
    private IGameSaveInteractor gameSaveInteractor;
    private bool pendingLoadApplied;

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
        PlayerCompositionResult playerComposition = PlayerComposition.Compose(
            playerTransform,
            playerMovement,
            playerView,
            inputService);
        playerTransform = playerComposition.PlayerTransform;
        playerView = playerComposition.PlayerView;

        HudComposition.Configure(hudView, playerComposition.PlayerHealth, playerCombatSystem);
        gameSaveInteractor = SaveComposition.BuildAndRegister(
            GameEntryPoint.Services,
            playerTransform,
            playerComposition.PlayerHealth,
            playerComposition.PlayerMana,
            playerComposition.PlayerStats,
            this);
        pauseMenuController = PauseMenuComposition.BuildAndInitialize(
            pauseMenuView,
            inputService,
            gameSaveInteractor,
            GameEntryPoint.Services.SceneLoader,
            gameplayComponentsToToggle,
            playerMovement,
            playerCombatSystem,
            playerAnimationController,
            mainMenuSceneName,
            this);
        victoryFlowController = VictoryComposition.BuildAndInitialize(
            victoryView,
            gameplayEventDirector,
            inputService,
            gameplayComponentsToToggle,
            playerMovement,
            playerCombatSystem,
            playerAnimationController,
            this);
    }

    private void Start()
    {
        pendingLoadApplied = gameSaveInteractor == null || gameSaveInteractor.ApplyPendingLoadedGame();
    }

    private void OnDestroy()
    {
        pauseMenuController?.Dispose();
        victoryFlowController?.Dispose();

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
        if (!pendingLoadApplied && gameSaveInteractor != null)
        {
            pendingLoadApplied = gameSaveInteractor.ApplyPendingLoadedGame();
        }

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

        if (victoryView == null)
        {
            victoryView = FindFirstObjectByType<VictoryView>();
        }

        if (gameplayEventDirector == null)
        {
            gameplayEventDirector = FindFirstObjectByType<GameplayEventDirector>();
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
}
