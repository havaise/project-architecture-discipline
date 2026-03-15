using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputService : MonoBehaviour, IInputService
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string gameplayMapName = "Gameplay";

    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string lookActionName = "Look";
    [SerializeField] private string sprintActionName = "Sprint";
    [SerializeField] private string physicalAttackActionName = "PhysicalAttack";
    [SerializeField] private string magicAttackActionName = "MagicAttack";
    [SerializeField] private string jumpActionName = "Jump";
    [SerializeField] private string interactActionName = "Interact";
    [SerializeField] private string pauseActionName = "Pause";

    private InputActionMap gameplayMap;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction physicalAttackAction;
    private InputAction magicAttackAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    private InputAction pauseAction;

    private bool callbacksBound;

    public Vector2 Move => moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public Vector2 Look => lookAction?.ReadValue<Vector2>() ?? Vector2.zero;

    public event Action AttackStarted;
    public event Action PhysicalAttackStarted;
    public event Action MagicAttackStarted;
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

    public bool IsSprintPressed()
    {
        return sprintAction != null && sprintAction.IsPressed();
    }

    public bool IsAttackPressed()
    {
        return IsPhysicalAttackPressed();
    }

    public bool IsPhysicalAttackPressed()
    {
        return physicalAttackAction != null && physicalAttackAction.WasPressedThisFrame();
    }

    public bool IsMagicAttackPressed()
    {
        return magicAttackAction != null && magicAttackAction.WasPressedThisFrame();
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
        sprintAction = FindAction(sprintActionName);
        physicalAttackAction = FindAction(physicalAttackActionName);
        magicAttackAction = FindAction(magicAttackActionName);
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

        if (physicalAttackAction != null)
        {
            physicalAttackAction.started += OnPhysicalAttackStarted;
        }

        if (magicAttackAction != null)
        {
            magicAttackAction.started += OnMagicAttackStarted;
        }

        if (jumpAction != null)
        {
            jumpAction.started += OnJumpStarted;
        }

        if (interactAction != null)
        {
            interactAction.started += OnInteractStarted;
        }

        if (pauseAction != null)
        {
            pauseAction.started += OnPauseStarted;
        }

        callbacksBound = true;
    }

    private void UnbindCallbacks()
    {
        if (!callbacksBound)
        {
            return;
        }

        if (physicalAttackAction != null)
        {
            physicalAttackAction.started -= OnPhysicalAttackStarted;
        }

        if (magicAttackAction != null)
        {
            magicAttackAction.started -= OnMagicAttackStarted;
        }

        if (jumpAction != null)
        {
            jumpAction.started -= OnJumpStarted;
        }

        if (interactAction != null)
        {
            interactAction.started -= OnInteractStarted;
        }

        if (pauseAction != null)
        {
            pauseAction.started -= OnPauseStarted;
        }

        callbacksBound = false;
    }

    private void OnPhysicalAttackStarted(InputAction.CallbackContext _)
    {
        AttackStarted?.Invoke();
        PhysicalAttackStarted?.Invoke();
    }

    private void OnMagicAttackStarted(InputAction.CallbackContext _)
    {
        MagicAttackStarted?.Invoke();
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
