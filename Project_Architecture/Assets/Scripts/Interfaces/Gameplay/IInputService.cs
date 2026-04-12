public interface IInputService
{
    UnityEngine.Vector2 Move { get; }
    UnityEngine.Vector2 Look { get; }

    bool IsSprintPressed();
    bool IsAttackPressed();
    bool IsPhysicalAttackPressed();
    bool IsMagicAttackPressed();
    bool IsJumpPressed();
    bool IsInteractPressed();
    bool IsPausePressed();

    event System.Action AttackStarted;
    event System.Action PhysicalAttackStarted;
    event System.Action MagicAttackStarted;
    event System.Action JumpStarted;
    event System.Action InteractStarted;
    event System.Action PauseStarted;

    void EnableGameplay();
    void DisableGameplay();
}
