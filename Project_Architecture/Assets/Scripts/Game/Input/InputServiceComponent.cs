using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputServiceComponent : MonoBehaviour, IInputService
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
    [SerializeField] private bool lockAndHideCursorOnEnable = true;

    private InputActionInputService runtimeService;

    public Vector2 Move => runtimeService != null ? runtimeService.Move : Vector2.zero;
    public Vector2 Look => runtimeService != null ? runtimeService.Look : Vector2.zero;

    public event Action AttackStarted;
    public event Action PhysicalAttackStarted;
    public event Action MagicAttackStarted;
    public event Action JumpStarted;
    public event Action InteractStarted;
    public event Action PauseStarted;

    private void Awake()
    {
        TryCreateRuntimeService();
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
        if (runtimeService == null)
        {
            return;
        }

        runtimeService.AttackStarted -= OnAttackStarted;
        runtimeService.PhysicalAttackStarted -= OnPhysicalAttackStarted;
        runtimeService.MagicAttackStarted -= OnMagicAttackStarted;
        runtimeService.JumpStarted -= OnJumpStarted;
        runtimeService.InteractStarted -= OnInteractStarted;
        runtimeService.PauseStarted -= OnPauseStarted;
        runtimeService.Dispose();
        runtimeService = null;
    }

    public bool IsSprintPressed()
    {
        return runtimeService != null && runtimeService.IsSprintPressed();
    }

    public bool IsAttackPressed()
    {
        return runtimeService != null && runtimeService.IsAttackPressed();
    }

    public bool IsPhysicalAttackPressed()
    {
        return runtimeService != null && runtimeService.IsPhysicalAttackPressed();
    }

    public bool IsMagicAttackPressed()
    {
        return runtimeService != null && runtimeService.IsMagicAttackPressed();
    }

    public bool IsJumpPressed()
    {
        return runtimeService != null && runtimeService.IsJumpPressed();
    }

    public bool IsInteractPressed()
    {
        return runtimeService != null && runtimeService.IsInteractPressed();
    }

    public bool IsPausePressed()
    {
        return runtimeService != null && runtimeService.IsPausePressed();
    }

    public void EnableGameplay()
    {
        if (!TryCreateRuntimeService())
        {
            return;
        }

        runtimeService.EnableGameplay();

        if (lockAndHideCursorOnEnable)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void DisableGameplay()
    {
        runtimeService?.DisableGameplay();
    }

    private bool TryCreateRuntimeService()
    {
        if (runtimeService != null)
        {
            return true;
        }

        try
        {
            runtimeService = new InputActionInputService(
                inputActions,
                gameplayMapName,
                moveActionName,
                lookActionName,
                sprintActionName,
                physicalAttackActionName,
                magicAttackActionName,
                jumpActionName,
                interactActionName,
                pauseActionName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"InputServiceComponent: {ex.Message}", this);
            runtimeService = null;
            return false;
        }

        runtimeService.AttackStarted += OnAttackStarted;
        runtimeService.PhysicalAttackStarted += OnPhysicalAttackStarted;
        runtimeService.MagicAttackStarted += OnMagicAttackStarted;
        runtimeService.JumpStarted += OnJumpStarted;
        runtimeService.InteractStarted += OnInteractStarted;
        runtimeService.PauseStarted += OnPauseStarted;
        return true;
    }

    private void OnAttackStarted()
    {
        AttackStarted?.Invoke();
    }

    private void OnPhysicalAttackStarted()
    {
        PhysicalAttackStarted?.Invoke();
    }

    private void OnMagicAttackStarted()
    {
        MagicAttackStarted?.Invoke();
    }

    private void OnJumpStarted()
    {
        JumpStarted?.Invoke();
    }

    private void OnInteractStarted()
    {
        InteractStarted?.Invoke();
    }

    private void OnPauseStarted()
    {
        PauseStarted?.Invoke();
    }
}
