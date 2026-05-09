using System;
using UnityEngine;

public sealed class EnemyAgent
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
    private readonly EnemyBehaviourConfig behaviourConfig;
    private readonly bool autoFindPlayer;
    private readonly bool enableCombatDebugLogs;
    private readonly Func<int> getDamage;
    private readonly Func<float> getHealthRatio;
    private readonly Action<EnemyAttackResult, float> handleAttackResult;
    private readonly StateMachine<EnemyAgent> stateMachine;

    public EnemyAgent(
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
        EnemyBehaviourConfig behaviourConfig,
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
        this.behaviourConfig = behaviourConfig;
        this.autoFindPlayer = autoFindPlayer;
        this.getDamage = getDamage;
        this.getHealthRatio = getHealthRatio;
        this.handleAttackResult = handleAttackResult;
        this.enableCombatDebugLogs = enableCombatDebugLogs;
        stateMachine = new StateMachine<EnemyAgent>(this);
    }

    public Transform CurrentTarget { get; private set; }
    public bool IsMoving => aiModel.IsMoving;
    public float MoveSpeedNormalized => aiModel.MoveSpeedNormalized;
    public string CurrentStateName => stateMachine.CurrentStateName;

    public void Initialize(float currentTime)
    {
        ResolveTarget(currentTime);
        stateMachine.SetInitialState(new EnemyRestState());
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

    private bool IsPeaceful()
    {
        return behaviourConfig.BehaviourMode == EnemyBehaviourMode.Peaceful;
    }

    private bool ShouldFlee()
    {
        if (behaviourConfig.FleeHealthThreshold <= 0f)
        {
            return false;
        }

        return getHealthRatio != null && getHealthRatio() <= behaviourConfig.FleeHealthThreshold;
    }

    private bool CanStopFlee()
    {
        float exitThreshold = Mathf.Max(behaviourConfig.FleeHealthThreshold, behaviourConfig.FleeExitHealthThreshold);
        return getHealthRatio == null || getHealthRatio() >= exitThreshold;
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

    private void FleeFromTarget(float deltaTime)
    {
        if (CurrentTarget == null)
        {
            StopAndRotateIdle(deltaTime);
            return;
        }

        Vector3 awayDirection = transform.position - CurrentTarget.position;
        awayDirection.y = 0f;
        if (awayDirection.sqrMagnitude <= 0.0001f)
        {
            awayDirection = -transform.forward;
        }

        Vector3 fleeDestination = transform.position
            + awayDirection.normalized * Mathf.Max(2f, behaviourConfig.FleeDistance);
        movementMotor.Chase(
            fleeDestination,
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
            Color.white,
            null,
            out float cooldownRemaining);

        handleAttackResult?.Invoke(result, cooldownRemaining);
        return result;
    }

    private void ChangeToRest()
    {
        stateMachine.ChangeState(new EnemyRestState());
    }

    private void ChangeToAggression()
    {
        stateMachine.ChangeState(new EnemyAggressionState());
    }

    private void ChangeToAttack()
    {
        stateMachine.ChangeState(new EnemyAttackState());
    }

    private void ChangeToFlee()
    {
        stateMachine.ChangeState(new EnemyFleeState());
    }

    private sealed class EnemyRestState : IState<EnemyAgent>
    {
        public string Name => "Rest";

        public void Enter(EnemyAgent context)
        {
            context.movementMotor.Stop();
        }

        public void Tick(EnemyAgent context, float currentTime, float deltaTime)
        {
            bool hasTarget = context.ResolveTarget(currentTime);
            if (!hasTarget)
            {
                context.StopAndRotateIdle(deltaTime);
                return;
            }

            if (context.ShouldFlee())
            {
                context.ChangeToFlee();
                return;
            }

            if (context.IsPeaceful())
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

        public void Exit(EnemyAgent context) { }
    }

    private sealed class EnemyAggressionState : IState<EnemyAgent>
    {
        public string Name => "Aggression";

        public void Enter(EnemyAgent context) { }

        public void Tick(EnemyAgent context, float currentTime, float deltaTime)
        {
            if (!context.ResolveTarget(currentTime))
            {
                context.ChangeToRest();
                return;
            }

            if (context.ShouldFlee())
            {
                context.ChangeToFlee();
                return;
            }

            if (context.IsPeaceful())
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
                context.ChangeToAttack();
                return;
            }

            context.ChaseCurrentTarget(canSeeTarget, deltaTime);
        }

        public void Exit(EnemyAgent context) { }
    }

    private sealed class EnemyAttackState : IState<EnemyAgent>
    {
        public string Name => "Attack";

        public void Enter(EnemyAgent context)
        {
            context.movementMotor.Stop();
        }

        public void Tick(EnemyAgent context, float currentTime, float deltaTime)
        {
            if (!context.ResolveTarget(currentTime))
            {
                context.ChangeToRest();
                return;
            }

            if (context.ShouldFlee())
            {
                context.ChangeToFlee();
                return;
            }

            if (context.IsPeaceful())
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

            context.movementMotor.Stop();
            context.TryAttack(currentTime, attackSpeedMultiplier: 1f, damageMultiplier: 1f);
        }

        public void Exit(EnemyAgent context) { }
    }

    private sealed class EnemyFleeState : IState<EnemyAgent>
    {
        public string Name => "Flee";

        public void Enter(EnemyAgent context) { }

        public void Tick(EnemyAgent context, float currentTime, float deltaTime)
        {
            if (!context.ResolveTarget(currentTime))
            {
                context.ChangeToRest();
                return;
            }

            if (context.CanStopFlee())
            {
                context.ChangeToRest();
                return;
            }

            context.FleeFromTarget(deltaTime);
        }

        public void Exit(EnemyAgent context) { }
    }
}
