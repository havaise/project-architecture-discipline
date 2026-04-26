using System;
using UnityEngine;

public sealed class BossAgent
{
    private readonly Transform transform;
    private readonly EnemyAiModel aiModel;
    private readonly EnemyVisionSensor visionSensor;
    private readonly EnemyMovementMotor movementMotor;
    private readonly EnemyAttackSystem attackSystem;
    private readonly IEnemyTargetProvider targetProvider;
    private readonly EnemyVisionConfig visionConfig;
    private readonly EnemyMovementConfig movementConfig;
    private readonly EnemyAttackConfig attackConfig;
    private readonly EnemyProjectileConfig projectileConfig;
    private readonly BossCombatConfig bossCombatConfig;
    private readonly bool autoFindPlayer;
    private readonly bool enableCombatDebugLogs;
    private readonly Func<int> getDamage;
    private readonly Func<float> getHealthRatio;
    private readonly Action<EnemyAttackResult, float> handleAttackResult;
    private readonly StateMachine<BossAgent> stateMachine;
    private readonly IState<BossAgent> restState;
    private readonly IState<BossAgent> aggressionState;
    private readonly IState<BossAgent> attackState;
    private readonly IState<BossAgent> strongAttackState;

    private float nextStrongAttackTime;

    public BossAgent(
        Transform transform,
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
        BossCombatConfig bossCombatConfig,
        bool autoFindPlayer,
        Func<int> getDamage,
        Func<float> getHealthRatio,
        Action<EnemyAttackResult, float> handleAttackResult,
        bool enableCombatDebugLogs)
    {
        this.transform = transform != null
            ? transform
            : throw new ArgumentNullException(nameof(transform));
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
        this.bossCombatConfig = bossCombatConfig;
        this.autoFindPlayer = autoFindPlayer;
        this.getDamage = getDamage;
        this.getHealthRatio = getHealthRatio;
        this.handleAttackResult = handleAttackResult;
        this.enableCombatDebugLogs = enableCombatDebugLogs;

        stateMachine = new StateMachine<BossAgent>(this);
        restState = new BossRestState();
        aggressionState = new BossAggressionState();
        attackState = new BossAttackState();
        strongAttackState = new BossStrongAttackState();
    }

    public Transform CurrentTarget { get; private set; }
    public bool IsMoving => aiModel.IsMoving;
    public float MoveSpeedNormalized => aiModel.MoveSpeedNormalized;
    public bool IsProvoked { get; private set; }
    public string CurrentStateName => stateMachine.CurrentStateName;

    public void Initialize(float currentTime)
    {
        ResolveTarget(currentTime);
        nextStrongAttackTime = currentTime + Mathf.Max(0.1f, bossCombatConfig.StrongAttackCooldown);
        stateMachine.SetInitialState(restState);
    }

    public void SetProvoked(bool value)
    {
        IsProvoked = value;
    }

    public void Tick(float currentTime, float deltaTime)
    {
        stateMachine.Tick(currentTime, deltaTime);
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

    private bool CanSeeTarget(float currentTime)
    {
        if (CurrentTarget == null)
        {
            return false;
        }

        bool canSeeTarget = visionSensor.CanSee(
            CurrentTarget,
            visionConfig.ViewDistance,
            visionConfig.ViewAngle,
            visionConfig.EyeHeight,
            visionConfig.VisibilityBlockers);

        if (canSeeTarget)
        {
            aiModel.RememberTarget(CurrentTarget.position, currentTime);
        }

        return canSeeTarget;
    }

    private bool IsInAttackRange()
    {
        return CurrentTarget != null
            && attackSystem.IsInRange(
                CurrentTarget,
                attackConfig.AttackMode,
                attackConfig.MeleeAttackRange,
                attackConfig.RangedAttackRange);
    }

    private bool HasTargetMemory(float currentTime)
    {
        return aiModel.HasMemory(currentTime);
    }

    private float GetAttackSpeedMultiplier()
    {
        if (!bossCombatConfig.EnrageBelowHalfHealth)
        {
            return 1f;
        }

        float healthRatio = getHealthRatio != null ? getHealthRatio() : 1f;
        if (healthRatio >= 0.5f)
        {
            return 1f;
        }

        return Mathf.Max(1f, bossCombatConfig.EnragedAttackSpeedMultiplier);
    }

    private bool IsStrongAttackReady(float currentTime)
    {
        return currentTime >= nextStrongAttackTime;
    }

    private void MarkStrongAttackUsed(float currentTime)
    {
        nextStrongAttackTime = currentTime + Mathf.Max(0.1f, bossCombatConfig.StrongAttackCooldown);
    }

    private void StopAndRotateIdle(float deltaTime)
    {
        movementMotor.Stop();
        movementMotor.RotateIdle(
            movementConfig.PatrolLookAroundWhenIdle,
            movementConfig.IdleTurnSpeed,
            deltaTime);
    }

    private void ChaseCurrentTarget(bool canSeeTarget, float deltaTime)
    {
        if (CurrentTarget == null)
        {
            StopAndRotateIdle(deltaTime);
            return;
        }

        Vector3 chasePoint = aiModel.GetChasePoint(CurrentTarget.position, canSeeTarget);
        movementMotor.Chase(
            chasePoint,
            movementConfig.MoveSpeed,
            movementConfig.RotationSpeed,
            deltaTime);
    }

    private EnemyAttackResult TryAttack(float currentTime, float attackSpeedMultiplier, float damageMultiplier)
    {
        if (CurrentTarget == null)
        {
            return EnemyAttackResult.None;
        }

        int baseDamage = getDamage != null ? getDamage() : attackConfig.Damage;
        int damage = Mathf.Max(0, Mathf.RoundToInt(baseDamage * Mathf.Max(0.1f, damageMultiplier)));
        EnemyAttackResult result = attackSystem.TryAttack(
            CurrentTarget,
            attackConfig.AttackMode,
            damage,
            currentTime,
            attackSpeedMultiplier,
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
        return result;
    }

    private void ChangeToRest()
    {
        stateMachine.ChangeState(restState);
    }

    private void ChangeToAggression()
    {
        stateMachine.ChangeState(aggressionState);
    }

    private void ChangeToAttack()
    {
        stateMachine.ChangeState(attackState);
    }

    private void ChangeToStrongAttack()
    {
        stateMachine.ChangeState(strongAttackState);
    }

    private sealed class BossRestState : IState<BossAgent>
    {
        public string Name => "Rest";

        public void Enter(BossAgent context)
        {
            context.movementMotor.Stop();
        }

        public void Tick(BossAgent context, float currentTime, float deltaTime)
        {
            if (!context.ResolveTarget(currentTime))
            {
                context.StopAndRotateIdle(deltaTime);
                return;
            }

            if (!context.IsProvoked)
            {
                context.StopAndRotateIdle(deltaTime);
                return;
            }

            bool canSeeTarget = context.CanSeeTarget(currentTime);
            if (canSeeTarget || context.HasTargetMemory(currentTime))
            {
                context.ChangeToAggression();
                return;
            }

            context.StopAndRotateIdle(deltaTime);
        }

        public void Exit(BossAgent context) { }
    }

    private sealed class BossAggressionState : IState<BossAgent>
    {
        public string Name => "Aggression";

        public void Enter(BossAgent context) { }

        public void Tick(BossAgent context, float currentTime, float deltaTime)
        {
            if (!context.IsProvoked)
            {
                context.ChangeToRest();
                return;
            }

            if (!context.ResolveTarget(currentTime))
            {
                context.ChangeToRest();
                return;
            }

            bool canSeeTarget = context.CanSeeTarget(currentTime);
            if (!canSeeTarget && !context.HasTargetMemory(currentTime))
            {
                context.ChangeToRest();
                return;
            }

            if (canSeeTarget && context.IsInAttackRange())
            {
                if (context.IsStrongAttackReady(currentTime))
                {
                    context.ChangeToStrongAttack();
                    return;
                }

                context.ChangeToAttack();
                return;
            }

            context.ChaseCurrentTarget(canSeeTarget, deltaTime);
        }

        public void Exit(BossAgent context) { }
    }

    private sealed class BossAttackState : IState<BossAgent>
    {
        public string Name => "Attack";

        public void Enter(BossAgent context)
        {
            context.movementMotor.Stop();
        }

        public void Tick(BossAgent context, float currentTime, float deltaTime)
        {
            if (!context.IsProvoked)
            {
                context.ChangeToRest();
                return;
            }

            if (!context.ResolveTarget(currentTime))
            {
                context.ChangeToRest();
                return;
            }

            bool canSeeTarget = context.CanSeeTarget(currentTime);
            if (!canSeeTarget || !context.IsInAttackRange())
            {
                context.ChangeToAggression();
                return;
            }

            if (context.IsStrongAttackReady(currentTime))
            {
                context.ChangeToStrongAttack();
                return;
            }

            context.movementMotor.Stop();
            context.TryAttack(currentTime, context.GetAttackSpeedMultiplier(), 1f);
        }

        public void Exit(BossAgent context) { }
    }

    private sealed class BossStrongAttackState : IState<BossAgent>
    {
        public string Name => "StrongAttack";

        public void Enter(BossAgent context)
        {
            context.movementMotor.Stop();
        }

        public void Tick(BossAgent context, float currentTime, float deltaTime)
        {
            if (!context.IsProvoked)
            {
                context.ChangeToRest();
                return;
            }

            if (!context.ResolveTarget(currentTime))
            {
                context.ChangeToRest();
                return;
            }

            bool canSeeTarget = context.CanSeeTarget(currentTime);
            if (!canSeeTarget || !context.IsInAttackRange())
            {
                context.ChangeToAggression();
                return;
            }

            EnemyAttackResult result = context.TryAttack(
                currentTime,
                context.GetAttackSpeedMultiplier(),
                Mathf.Max(1f, context.bossCombatConfig.StrongAttackDamageMultiplier));

            if (result != EnemyAttackResult.Cooldown && result != EnemyAttackResult.None)
            {
                context.MarkStrongAttackUsed(currentTime);
            }

            context.ChangeToAttack();
        }

        public void Exit(BossAgent context) { }
    }
}
