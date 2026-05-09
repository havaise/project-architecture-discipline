using System;
using UnityEngine;

public sealed class BossAgent
{
    private const float DefaultRecoverDuration = 0.45f;
    private const float DefaultStrongRecoverDuration = 0.8f;
    private const float DefaultDodgeDuration = 0.4f;
    private const float DefaultDodgeCooldown = 2.4f;
    private const float DefaultChaseDodgeDelay = 2.2f;
    private const float DefaultDodgeDistance = 2f;
    private const float DodgeSpeedMultiplier = 1.35f;
    private const float DodgeTurnMultiplier = 1.5f;

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
    private readonly Color projectileTint;
    private readonly GameObject projectileHitVfx;

    private float nextStrongAttackTime;
    private float nextDodgeTime;
    private float recoverUntilTime;
    private float dodgeUntilTime;
    private float continuousChaseTime;
    private bool dodgeLeft = true;

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
        Color projectileTint,
        GameObject projectileHitVfx,
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
        this.projectileTint = projectileTint;
        this.projectileHitVfx = projectileHitVfx;
        this.enableCombatDebugLogs = enableCombatDebugLogs;

        stateMachine = new StateMachine<BossAgent>(this);
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
        nextDodgeTime = currentTime;
        stateMachine.SetInitialState(new BossRestState());
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

    private bool ShouldDodge(float currentTime)
    {
        if (CurrentTarget == null || currentTime < nextDodgeTime)
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, CurrentTarget.position);
        float dodgeTriggerDistance = Mathf.Max(1.2f, attackConfig.MeleeAttackRange * 0.9f);
        return distance <= dodgeTriggerDistance;
    }

    private bool ShouldDodgeFromLongChase(float currentTime)
    {
        if (CurrentTarget == null || currentTime < nextDodgeTime)
        {
            return false;
        }

        return continuousChaseTime >= DefaultChaseDodgeDelay;
    }

    private void MarkDodgeUsed(float currentTime)
    {
        nextDodgeTime = currentTime + DefaultDodgeCooldown;
    }

    private void StopAndRotateIdle(float deltaTime)
    {
        movementMotor.Stop();
        movementMotor.RotateIdle(
            movementConfig.PatrolLookAroundWhenIdle,
            movementConfig.IdleTurnSpeed,
            deltaTime);
    }

    private void ChaseCurrentTarget(bool canSeeTarget, float currentTime, float deltaTime)
    {
        if (CurrentTarget == null)
        {
            StopAndRotateIdle(deltaTime);
            return;
        }

        Vector3 chasePoint = aiModel.GetChasePoint(CurrentTarget.position, canSeeTarget);
        float stoppingDistance = attackConfig.AttackMode == EnemyAttackKind.Ranged
            ? attackConfig.RangedAttackRange
            : attackConfig.MeleeAttackRange;
        movementMotor.Chase(
            chasePoint,
            movementConfig.MoveSpeed,
            movementConfig.NavAcceleration,
            movementConfig.NavAngularSpeed,
            Mathf.Max(movementConfig.NavStoppingDistance, stoppingDistance * 0.9f),
            movementConfig.NavPathRepathInterval,
            currentTime);
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
            projectileTint,
            projectileHitVfx,
            out float cooldownRemaining);

        handleAttackResult?.Invoke(result, cooldownRemaining);
        return result;
    }

    private void PerformDodge(float currentTime, float deltaTime)
    {
        if (CurrentTarget == null)
        {
            StopAndRotateIdle(deltaTime);
            return;
        }

        Vector3 toTarget = CurrentTarget.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            toTarget = transform.forward;
        }

        Vector3 side = Vector3.Cross(Vector3.up, toTarget.normalized);
        if (!dodgeLeft)
        {
            side = -side;
        }

        Vector3 dodgePoint = transform.position + side * DefaultDodgeDistance;
        movementMotor.Chase(
            dodgePoint,
            movementConfig.MoveSpeed * DodgeSpeedMultiplier,
            movementConfig.NavAcceleration * DodgeSpeedMultiplier,
            movementConfig.NavAngularSpeed * DodgeTurnMultiplier,
            movementConfig.NavStoppingDistance,
            movementConfig.NavPathRepathInterval,
            currentTime);
    }

    private void BeginSearch(float currentTime)
    {
        stateMachine.ChangeState(new BossSearchState());
    }

    private void BeginRecover(float currentTime, float duration)
    {
        recoverUntilTime = currentTime + Mathf.Max(0.05f, duration);
        stateMachine.ChangeState(new BossRecoverState());
    }

    private bool IsRecoverDone(float currentTime)
    {
        return currentTime >= recoverUntilTime;
    }

    private void BeginDodge(float currentTime)
    {
        dodgeUntilTime = currentTime + DefaultDodgeDuration;
        dodgeLeft = !dodgeLeft;
        MarkDodgeUsed(currentTime);
        ResetChaseTimer();
        stateMachine.ChangeState(new BossDodgeState());
    }

    private bool IsDodgeDone(float currentTime)
    {
        return currentTime >= dodgeUntilTime;
    }

    private void UpdateChaseTimer(float deltaTime)
    {
        continuousChaseTime += Mathf.Max(0f, deltaTime);
    }

    private void ResetChaseTimer()
    {
        continuousChaseTime = 0f;
    }

    private void ChangeToRest()
    {
        stateMachine.ChangeState(new BossRestState());
    }

    private void ChangeToAggression()
    {
        stateMachine.ChangeState(new BossAggressionState());
    }

    private void ChangeToAttack()
    {
        stateMachine.ChangeState(new BossAttackState());
    }

    private void ChangeToStrongAttack()
    {
        stateMachine.ChangeState(new BossStrongAttackState());
    }

    private sealed class BossRestState : IState<BossAgent>
    {
        public string Name => "Rest";

        public void Enter(BossAgent context)
        {
            context.movementMotor.Stop();
            context.ResetChaseTimer();
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

        public void Enter(BossAgent context)
        {
            context.ResetChaseTimer();
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
            bool hasMemory = context.HasTargetMemory(currentTime);

            if (!canSeeTarget)
            {
                if (hasMemory)
                {
                    context.BeginSearch(currentTime);
                }
                else
                {
                    context.ChangeToRest();
                }

                return;
            }

            if (context.IsInAttackRange())
            {
                context.ResetChaseTimer();

                if (context.IsStrongAttackReady(currentTime))
                {
                    context.ChangeToStrongAttack();
                    return;
                }

                if (context.ShouldDodge(currentTime))
                {
                    context.BeginDodge(currentTime);
                    return;
                }

                context.ChangeToAttack();
                return;
            }

            context.UpdateChaseTimer(deltaTime);
            if (context.ShouldDodgeFromLongChase(currentTime))
            {
                context.BeginDodge(currentTime);
                return;
            }

            context.ChaseCurrentTarget(canSeeTarget, currentTime, deltaTime);
        }

        public void Exit(BossAgent context) { }
    }

    private sealed class BossSearchState : IState<BossAgent>
    {
        public string Name => "Search";

        public void Enter(BossAgent context)
        {
            context.ResetChaseTimer();
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
            if (canSeeTarget)
            {
                context.ResetChaseTimer();
                context.ChangeToAggression();
                return;
            }

            if (!context.HasTargetMemory(currentTime))
            {
                context.ResetChaseTimer();
                context.ChangeToRest();
                return;
            }

            context.UpdateChaseTimer(deltaTime);
            context.ChaseCurrentTarget(canSeeTarget: false, currentTime, deltaTime);
        }

        public void Exit(BossAgent context) { }
    }

    private sealed class BossAttackState : IState<BossAgent>
    {
        public string Name => "Attack";

        public void Enter(BossAgent context)
        {
            context.movementMotor.Stop();
            context.ResetChaseTimer();
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
                context.BeginRecover(currentTime, DefaultRecoverDuration);
                return;
            }

            if (context.IsStrongAttackReady(currentTime))
            {
                context.ChangeToStrongAttack();
                return;
            }

            if (context.ShouldDodge(currentTime))
            {
                context.BeginDodge(currentTime);
                return;
            }

            context.movementMotor.Stop();
            EnemyAttackResult result = context.TryAttack(currentTime, context.GetAttackSpeedMultiplier(), 1f);
            if (result != EnemyAttackResult.Cooldown && result != EnemyAttackResult.None)
            {
                context.BeginRecover(currentTime, DefaultRecoverDuration);
            }
        }

        public void Exit(BossAgent context) { }
    }

    private sealed class BossStrongAttackState : IState<BossAgent>
    {
        public string Name => "StrongAttack";

        public void Enter(BossAgent context)
        {
            context.movementMotor.Stop();
            context.ResetChaseTimer();
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
                context.BeginRecover(currentTime, DefaultStrongRecoverDuration);
                return;
            }

            EnemyAttackResult result = context.TryAttack(
                currentTime,
                context.GetAttackSpeedMultiplier(),
                Mathf.Max(1f, context.bossCombatConfig.StrongAttackDamageMultiplier));

            if (result != EnemyAttackResult.Cooldown && result != EnemyAttackResult.None)
            {
                context.MarkStrongAttackUsed(currentTime);
                context.BeginRecover(currentTime, DefaultStrongRecoverDuration);
                return;
            }

            context.ChangeToAttack();
        }

        public void Exit(BossAgent context) { }
    }

    private sealed class BossDodgeState : IState<BossAgent>
    {
        public string Name => "Dodge";

        public void Enter(BossAgent context)
        {
            context.ResetChaseTimer();
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

            if (context.IsDodgeDone(currentTime))
            {
                context.ChangeToAggression();
                return;
            }

            context.PerformDodge(currentTime, deltaTime);
        }

        public void Exit(BossAgent context)
        {
            context.movementMotor.Stop();
        }
    }

    private sealed class BossRecoverState : IState<BossAgent>
    {
        public string Name => "Recover";

        public void Enter(BossAgent context)
        {
            context.movementMotor.Stop();
            context.ResetChaseTimer();
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

            if (context.IsRecoverDone(currentTime))
            {
                context.ChangeToAggression();
                return;
            }

            context.StopAndRotateIdle(deltaTime);
        }

        public void Exit(BossAgent context) { }
    }
}
