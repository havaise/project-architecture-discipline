using UnityEngine;

public class GameOverController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonoBehaviour playerHealthSource;
    [SerializeField] private MonoBehaviour inputServiceSource;
    [SerializeField] private GameOverView view;

    [Header("Control Components To Disable")]
    [SerializeField] private MonoBehaviour[] controlComponents;

    [Header("Behavior")]
    [SerializeField] private bool autoFindPlayerByTag = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool pauseTimeOnGameOver = true;
    [SerializeField] private bool unlockCursorOnGameOver = true;

    private IHealth playerHealth;
    private IInputService inputService;
    private GameOverFlowController flowController;

    private void Awake()
    {
        TryResolveReferences();
        if (!EnsureView())
        {
            return;
        }

        if (playerHealth == null)
        {
            Debug.LogError("GameOverController: Player health source is not assigned.", this);
            return;
        }

        flowController = new GameOverFlowController(
            new GameOverModel(),
            view,
            playerHealth,
            inputService,
            controlComponents,
            pauseTimeOnGameOver,
            unlockCursorOnGameOver);
    }

    private void OnEnable()
    {
        flowController?.Initialize();
    }

    private void OnDisable()
    {
        flowController?.Dispose();
    }

    private void TryResolveReferences()
    {
        if (playerHealth == null && autoFindPlayerByTag)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                HealthComponent healthComponent = playerObject.GetComponentInChildren<HealthComponent>();
                if (healthComponent != null)
                {
                    playerHealthSource = healthComponent;
                    playerHealth = healthComponent;
                }
            }
        }

        if (playerHealth == null)
        {
            playerHealth = playerHealthSource as IHealth;
        }

        InputServiceResolver.TryResolve(ref inputService, ref inputServiceSource);
    }

    private bool EnsureView()
    {
        if (view == null)
        {
            view = GetComponentInChildren<GameOverView>(true);
        }

        if (view == null)
        {
            Debug.LogError("GameOverController: GameOverView is not assigned.", this);
            return false;
        }

        return true;
    }
}
