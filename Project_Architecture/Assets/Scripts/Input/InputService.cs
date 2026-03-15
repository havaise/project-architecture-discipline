using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputService : MonoBehaviour, IInputService
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string gameplayMapName = "Gameplay";

    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string lookActionName = "Look";
    [SerializeField] private string attackActionName = "Attack";
    [SerializeField] private string jumpActionName = "Jump";
    [SerializeField] private string interactActionName = "Interact";
    [SerializeField] private string pauseActionName = "Pause";

    private InputActionMap gameplayMap;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction attackAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction pauseAction;

    private bool callbacksBound;

    public Vector2 Move => moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public Vector2 Look => lookAction?.ReadValue<Vector2>() ?? Vector2.zero;

    public event Action AttackStarted;
    public event Action JumpStarted;
    public event Action InteractStarted;
    public event Action PauseStarted;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        EnableGameplay();
    }

    private void OnDisable()
    {
        DisableGameplay();
    }

    private void OnDestroy()
    {
        UnbindCallbacks();
    }

    public bool IsAttackPressed()
    {
        return attackAction != null && attackAction.WasPressedThisFrame();
    }

    public bool IsJumpPressed()
    {
        return jumpAction != null && jumpAction.WasPressedThisFrame();
    }

    public bool IsInteractPressed()
    {
        return interactAction != null && interactAction.WasPressedThisFrame();
    }

    public bool IsPausePressed()
    {
        return pauseAction != null && pauseAction.WasPressedThisFrame();
    }

    public void EnableGameplay()
    {
        Initialize();

        if (gameplayMap == null)
        {
            return;
        }

        BindCallbacks();
        gameplayMap.Enable();
    }

    public void DisableGameplay()
    {
        if (gameplayMap == null)
        {
            return;
        }

        gameplayMap.Disable();
        UnbindCallbacks();
    }

    private void Initialize()
    {
        if (gameplayMap != null)
        {
            return;
        }

        if (inputActions == null)
        {
            Debug.LogError("InputService: InputActionAsset is not assigned.", this);
            return;
        }

        gameplayMap = inputActions.FindActionMap(gameplayMapName, false);
        if (gameplayMap == null)
        {
            Debug.LogError($"InputService: Action map '{gameplayMapName}' not found.", this);
            return;
        }

        moveAction = FindAction(moveActionName);
        lookAction = FindAction(lookActionName);
        attackAction = FindAction(attackActionName);
        jumpAction = FindAction(jumpActionName);
        interactAction = FindAction(interactActionName);
        pauseAction = FindAction(pauseActionName);
    }

    private InputAction FindAction(string actionName)
    {
        InputAction action = gameplayMap.FindAction(actionName, false);
        if (action == null)
        {
            Debug.LogError($"InputService: Action '{actionName}' not found in map '{gameplayMapName}'.", this);
        }

        return action;
    }

    private void BindCallbacks()
    {
        if (callbacksBound)
        {
            return;
        }

        attackAction?.started += OnAttackStarted;
        jumpAction?.started += OnJumpStarted;
        interactAction?.started += OnInteractStarted;
        pauseAction?.started += OnPauseStarted;
        callbacksBound = true;
    }

    private void UnbindCallbacks()
    {
        if (!callbacksBound)
        {
            return;
        }

        attackAction?.started -= OnAttackStarted;
        jumpAction?.started -= OnJumpStarted;
        interactAction?.started -= OnInteractStarted;
        pauseAction?.started -= OnPauseStarted;

        callbacksBound = false;
    }

    private void OnAttackStarted(InputAction.CallbackContext _)
    {
        AttackStarted?.Invoke();
    }

    private void OnJumpStarted(InputAction.CallbackContext _)
    {
        JumpStarted?.Invoke();
    }

    private void OnInteractStarted(InputAction.CallbackContext _)
    {
        InteractStarted?.Invoke();
    }

    private void OnPauseStarted(InputAction.CallbackContext _)
    {
        PauseStarted?.Invoke();
    }
}
