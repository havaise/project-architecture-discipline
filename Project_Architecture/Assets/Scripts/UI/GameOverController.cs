using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthComponent playerHealth;
    [SerializeField] private InputService inputServiceSource;
    [SerializeField] private GameObject restartMenuRoot;

    [Header("Control Components To Disable")]
    [SerializeField] private MonoBehaviour[] controlComponents;

    [Header("Behavior")]
    [SerializeField] private bool autoFindPlayerByTag = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool pauseTimeOnGameOver = true;
    [SerializeField] private bool unlockCursorOnGameOver = true;

    private bool gameOverTriggered;
    private IInputService inputService;

    private void Awake()
    {
        if (restartMenuRoot != null)
        {
            restartMenuRoot.SetActive(false);
        }

        TryResolveReferences();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void TryResolveReferences()
    {
        if (playerHealth == null && autoFindPlayerByTag)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                playerHealth = playerObject.GetComponentInChildren<HealthComponent>();
            }
        }

        ResolveInputService();
    }

    private void Subscribe()
    {
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.Died += OnPlayerDied;
    }

    private void Unsubscribe()
    {
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.Died -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        if (gameOverTriggered)
        {
            return;
        }

        gameOverTriggered = true;

        if (inputService != null)
        {
            inputService.DisableGameplay();
        }

        DisableControls();

        if (pauseTimeOnGameOver)
        {
            Time.timeScale = 0f;
        }

        if (restartMenuRoot != null)
        {
            restartMenuRoot.SetActive(true);
        }

        if (unlockCursorOnGameOver)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void DisableControls()
    {
        if (controlComponents == null)
        {
            return;
        }

        for (int i = 0; i < controlComponents.Length; i++)
        {
            MonoBehaviour component = controlComponents[i];
            if (component != null)
            {
                component.enabled = false;
            }
        }
    }

    public void SetInputService(IInputService service)
    {
        inputService = service;
    }

    private void ResolveInputService()
    {
        if (inputService != null)
        {
            return;
        }

        if (inputServiceSource == null)
        {
            inputServiceSource = FindFirstObjectByType<InputService>();
        }

        inputService = inputServiceSource;
    }
}
