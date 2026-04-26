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

public sealed class EnemyBrain
{
    private readonly EnemyAiModel aiModel;
    private readonly EnemyAttackSystem attackSystem;

    public EnemyBrain(EnemyAiModel aiModel, EnemyAttackSystem attackSystem)
    {
        this.aiModel = aiModel;
        this.attackSystem = attackSystem;
    }

    public EnemyBrainDecision Evaluate(
        Transform target,
        bool canSeeTarget,
        float currentTime,
        EnemyAttackKind attackKind,
        float meleeAttackRange,
        float rangedAttackRange)
    {
        if (target == null)
        {
            return new EnemyBrainDecision(EnemyBrainAction.Idle, Vector3.zero, false);
        }

        if (canSeeTarget)
        {
            aiModel.RememberTarget(target.position, currentTime);
        }

        if (!canSeeTarget && !aiModel.HasMemory(currentTime))
        {
            return new EnemyBrainDecision(EnemyBrainAction.Idle, Vector3.zero, false);
        }

        if (!canSeeTarget || !attackSystem.IsInRange(target, attackKind, meleeAttackRange, rangedAttackRange))
        {
            Vector3 chasePoint = aiModel.GetChasePoint(target.position, canSeeTarget);
            return new EnemyBrainDecision(EnemyBrainAction.Chase, chasePoint, canSeeTarget);
        }

        return new EnemyBrainDecision(EnemyBrainAction.Attack, Vector3.zero, true);
    }
}
