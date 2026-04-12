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

    public bool TryConsumeAttack(float currentTime, out float cooldownRemaining)
    {
        if (currentTime < nextAttackTime)
        {
            cooldownRemaining = nextAttackTime - currentTime;
            return false;
        }

        nextAttackTime = currentTime + attackCooldown;
        cooldownRemaining = 0f;
        return true;
    }

    public void SetMoveState(bool moving, float speed01)
    {
        isMoving = moving;
        moveSpeedNormalized = Mathf.Clamp01(speed01);
    }
}
