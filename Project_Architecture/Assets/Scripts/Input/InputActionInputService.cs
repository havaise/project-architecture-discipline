using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputActionInputService : IInputService, IDisposable
{
    private readonly InputActionMap gameplayMap;
    private readonly InputAction moveAction;
    private readonly InputAction lookAction;
    private readonly InputAction sprintAction;
    private readonly InputAction physicalAttackAction;
    private readonly InputAction magicAttackAction;
    private readonly InputAction jumpAction;
    private readonly InputAction interactAction;
    private readonly InputAction pauseAction;

    private bool callbacksBound;

    public InputActionInputService(
        InputActionAsset inputActions,
        string gameplayMapName,
        string moveActionName,
        string lookActionName,
        string sprintActionName,
        string physicalAttackActionName,
        string magicAttackActionName,
        string jumpActionName,
        string interactActionName,
        string pauseActionName)
    {
        if (inputActions == null)
        {
            throw new ArgumentNullException(nameof(inputActions), "InputActionAsset is not assigned.");
        }

        gameplayMap = inputActions.FindActionMap(gameplayMapName, false);
        if (gameplayMap == null)
        {
            throw new InvalidOperationException($"Input action map '{gameplayMapName}' not found.");
        }

        moveAction = FindAction(gameplayMap, moveActionName);
        lookAction = FindAction(gameplayMap, lookActionName);
        sprintAction = FindAction(gameplayMap, sprintActionName);
        physicalAttackAction = FindAction(gameplayMap, physicalAttackActionName);
        magicAttackAction = FindAction(gameplayMap, magicAttackActionName);
        jumpAction = FindAction(gameplayMap, jumpActionName);
        interactAction = FindAction(gameplayMap, interactActionName);
        pauseAction = FindAction(gameplayMap, pauseActionName);
    }

    public Vector2 Move => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 Look => lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

    public event Action AttackStarted;
    public event Action PhysicalAttackStarted;
    public event Action MagicAttackStarted;
    public event Action JumpStarted;
    public event Action InteractStarted;
    public event Action PauseStarted;

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
        return physicalAttackAction != null && physicalAttackAction.IsPressed();
    }

    public bool IsMagicAttackPressed()
    {
        return magicAttackAction != null && magicAttackAction.IsPressed();
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
        BindCallbacks();
        gameplayMap.Enable();
    }

    public void DisableGameplay()
    {
        gameplayMap.Disable();
        UnbindCallbacks();
    }

    public void Dispose()
    {
        UnbindCallbacks();
    }

    private static InputAction FindAction(InputActionMap map, string actionName)
    {
        InputAction action = map.FindAction(actionName, false);
        if (action == null)
        {
            throw new InvalidOperationException($"Input action '{actionName}' not found in map '{map.name}'.");
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
