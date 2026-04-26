using System;
using UnityEngine;

public sealed class EnemyAgent
{
    private readonly EnemyAiModel aiModel;
    private readonly EnemyVisionSensor visionSensor;
    private readonly EnemyMovementMotor movementMotor;
    private readonly EnemyAttackSystem attackSystem;
    private readonly IEnemyTargetProvider targetProvider;
    private readonly EnemyVisionConfig visionConfig;
    private readonly EnemyMovementConfig movementConfig;
    private readonly EnemyAttackConfig attackConfig;
    private readonly EnemyProjectileConfig projectileConfig;
    private readonly bool autoFindPlayer;
    private readonly bool enableCombatDebugLogs;
    private readonly Func<int> getDamage;
    private readonly Action<EnemyAttackResult, float> handleAttackResult;

    public EnemyAgent(
        Transform initialTarget,
        EnemyAiModel aiModel,
        EnemyVisionSensor visionSensor,
        EnemyMovementMotor movementMotor,
        EnemyAttackSystem attackSystem,
        IEnemyTargetProvider targetProvider,
        EnemyVisionConfig visionConfig,
        EnemyMovementConfig movementConfig,
        EnemyAttackConfig attackConfig,
        EnemyProjectileConfig projectileConfig,
        bool autoFindPlayer,
        Func<int> getDamage,
        Action<EnemyAttackResult, float> handleAttackResult,
        bool enableCombatDebugLogs)
    {
        CurrentTarget = initialTarget;
        this.aiModel = aiModel;
        this.visionSensor = visionSensor;
        this.movementMotor = movementMotor;
        this.attackSystem = attackSystem;
        this.targetProvider = targetProvider;
        this.visionConfig = visionConfig;
        this.movementConfig = movementConfig;
        this.attackConfig = attackConfig;
        this.projectileConfig = projectileConfig;
        this.autoFindPlayer = autoFindPlayer;
        this.getDamage = getDamage;
        this.handleAttackResult = handleAttackResult;
        this.enableCombatDebugLogs = enableCombatDebugLogs;
    }

    public Transform CurrentTarget { get; private set; }
    public bool IsMoving => aiModel.IsMoving;
    public float MoveSpeedNormalized => aiModel.MoveSpeedNormalized;

    public void Initialize(float currentTime)
    {
        ResolveTarget(currentTime);
    }

    public void Tick(float currentTime, float deltaTime)
    {
        if (!ResolveTarget(currentTime))
        {
            movementMotor.Stop();
            movementMotor.RotateIdle(
                movementConfig.PatrolLookAroundWhenIdle,
                movementConfig.IdleTurnSpeed,
                deltaTime);
            return;
        }

        bool canSeeTarget = visionSensor.CanSee(
            CurrentTarget,
            visionConfig.ViewDistance,
            visionConfig.ViewAngle,
            visionConfig.EyeHeight,
            visionConfig.VisibilityBlockers);
        EnemyBrainDecision decision = aiModel.EvaluateDecision(
            CurrentTarget,
            canSeeTarget,
            attackSystem,
            currentTime,
            attackConfig.AttackMode,
            attackConfig.MeleeAttackRange,
            attackConfig.RangedAttackRange);

        if (decision.Action == EnemyBrainAction.Idle)
        {
            movementMotor.Stop();
            movementMotor.RotateIdle(
                movementConfig.PatrolLookAroundWhenIdle,
                movementConfig.IdleTurnSpeed,
                deltaTime);
            return;
        }

        if (decision.Action == EnemyBrainAction.Chase)
        {
            movementMotor.Chase(
                decision.ChasePoint,
                movementConfig.MoveSpeed,
                movementConfig.RotationSpeed,
                deltaTime);
            return;
        }

        movementMotor.Stop();
        TryAttack(decision.CanSeeTarget, currentTime);
    }

    private bool ResolveTarget(float currentTime)
    {
        Transform resolvedTarget = targetProvider.Resolve(CurrentTarget, autoFindPlayer);
        if (resolvedTarget == null)
        {
            return false;
        }

        if (resolvedTarget != CurrentTarget)
        {
            CurrentTarget = resolvedTarget;
            aiModel.RememberTarget(CurrentTarget.position, currentTime);
        }

        return true;
    }

    private void TryAttack(bool canSeeTarget, float currentTime)
    {
        if (CurrentTarget == null || !canSeeTarget)
        {
            return;
        }

        EnemyAttackResult result = attackSystem.TryAttack(
            CurrentTarget,
            attackConfig.AttackMode,
            getDamage(),
            currentTime,
            projectileConfig.ProjectileSpeed,
            projectileConfig.ProjectileLifetime,
            projectileConfig.ProjectileRadius,
            projectileConfig.ProjectileSpawnPoint,
            projectileConfig.ProjectileSpawnHeightOffset,
            projectileConfig.ProjectileSpawnForwardOffset,
            projectileConfig.ProjectileHitMask,
            enableCombatDebugLogs,
            out float cooldownRemaining);

        handleAttackResult?.Invoke(result, cooldownRemaining);
    }
}
