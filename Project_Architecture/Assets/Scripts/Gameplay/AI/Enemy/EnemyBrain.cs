using UnityEngine;

public enum EnemyBrainAction
{
    Idle = 0,
    Chase = 1,
    Attack = 2
}

public readonly struct EnemyBrainDecision
{
    public EnemyBrainDecision(EnemyBrainAction action, Vector3 chasePoint, bool canSeeTarget)
    {
        Action = action;
        ChasePoint = chasePoint;
        CanSeeTarget = canSeeTarget;
    }

    public EnemyBrainAction Action { get; }
    public Vector3 ChasePoint { get; }
    public bool CanSeeTarget { get; }
}
