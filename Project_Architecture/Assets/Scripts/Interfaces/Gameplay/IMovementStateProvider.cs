public interface IMovementStateProvider
{
    bool IsMoving { get; }
    float MoveSpeedNormalized { get; }
}
