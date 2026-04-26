using UnityEngine;

public sealed class EnemyAiModel
{
    private readonly float visibilityMemoryDuration;
    private readonly float attackCooldown;

    private float nextAttackTime;
    private float visibleUntilTime;
    private Vector3 lastKnownTargetPosition;
    private bool isMoving;
    private float moveSpeedNormalized;

    public EnemyAiModel(float visibilityMemoryDuration, float attackCooldown)
    {
        this.visibilityMemoryDuration = visibilityMemoryDuration;
        this.attackCooldown = attackCooldown;
    }

    public bool IsMoving => isMoving;
    public float MoveSpeedNormalized => moveSpeedNormalized;

    public void RememberTarget(Vector3 targetPosition, float time)
    {
        visibleUntilTime = time + visibilityMemoryDuration;
        lastKnownTargetPosition = targetPosition;
    }

    public bool HasMemory(float time)
    {
        return time <= visibleUntilTime;
    }

    public Vector3 GetChasePoint(Vector3 currentTargetPosition, bool canSeeTarget)
    {
        return canSeeTarget ? currentTargetPosition : lastKnownTargetPosition;
    }

    public bool TryConsumeAttack(float currentTime, out float cooldownRemaining, float attackSpeedMultiplier = 1f)
    {
        if (currentTime < nextAttackTime)
        {
            cooldownRemaining = nextAttackTime - currentTime;
            return false;
        }

        float cooldownDuration = attackCooldown;
        if (attackSpeedMultiplier > 0.01f)
        {
            cooldownDuration /= attackSpeedMultiplier;
        }

        nextAttackTime = currentTime + Mathf.Max(0.01f, cooldownDuration);
        cooldownRemaining = 0f;
        return true;
    }

    public EnemyBrainDecision EvaluateDecision(
        Transform target,
        bool canSeeTarget,
        EnemyAttackSystem attackSystem,
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
            RememberTarget(target.position, currentTime);
        }

        if (!canSeeTarget && !HasMemory(currentTime))
        {
            return new EnemyBrainDecision(EnemyBrainAction.Idle, Vector3.zero, false);
        }

        if (!canSeeTarget || !attackSystem.IsInRange(target, attackKind, meleeAttackRange, rangedAttackRange))
        {
            Vector3 chasePoint = GetChasePoint(target.position, canSeeTarget);
            return new EnemyBrainDecision(EnemyBrainAction.Chase, chasePoint, canSeeTarget);
        }

        return new EnemyBrainDecision(EnemyBrainAction.Attack, Vector3.zero, true);
    }

    public void SetMoveState(bool moving, float speed01)
    {
        isMoving = moving;
        moveSpeedNormalized = Mathf.Clamp01(speed01);
    }
}
