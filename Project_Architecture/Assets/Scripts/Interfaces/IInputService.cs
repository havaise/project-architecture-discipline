public interface IInputService
{
    UnityEngine.Vector2 Move { get; }
    UnityEngine.Vector2 Look { get; }

    bool IsAttackPressed();
    bool IsJumpPressed();
    bool IsInteractPressed();
    bool IsPausePressed();

    event System.Action AttackStarted;
    event System.Action JumpStarted;
    event System.Action InteractStarted;
    event System.Action PauseStarted;

    void EnableGameplay();
    void DisableGameplay();
}
