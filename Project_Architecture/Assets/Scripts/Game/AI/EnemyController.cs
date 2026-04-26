using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour, IMovementStateProvider, IEnemyAttackEvents, IDamageSource
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Vision")]
    [SerializeField] private EnemyVisionConfig visionConfig = default;

    [Header("Movement")]
    [SerializeField] private EnemyMovementConfig movementConfig = default;

    [Header("Attack")]
    [SerializeField] private EnemyAttackConfig attackConfig = default;

    [Header("Projectile")]
    [SerializeField] private EnemyProjectileConfig projectileConfig = default;

    [Header("Debug")]
    [SerializeField] private bool enableCombatDebugLogs;
    [SerializeField] private bool logCooldownBlocks;

    public event Action MeleeAttackPerformed;
    public event Action RangedAttackPerformed;

    public bool IsMoving => aiModel != null && aiModel.IsMoving;
    public float MoveSpeedNormalized => aiModel != null ? aiModel.MoveSpeedNormalized : 0f;

    private NavMeshAgent navMeshAgent;
    private Rigidbody body;
    private EnemyAiModel aiModel;
    private EnemyVisionSensor visionSensor;
    private EnemyMovementMotor movementMotor;
    private EnemyAttackSystem attackSystem;
    private EnemyBrain enemyBrain;
    private IEnemyTargetProvider targetProvider;
    private IEnemyProjectileFactory projectileFactory;

    private void Awake()
    {
        ApplyDefaultConfigsIfNeeded();

        navMeshAgent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        targetProvider = new UnityEnemyTargetProvider(playerTag);
        projectileFactory = new UnityEnemyProjectileFactory(projectileConfig.ProjectilePrefab, Log);
        aiModel = new EnemyAiModel(visionConfig.VisibilityMemoryDuration, attackConfig.AttackCooldown);
        visionSensor = new EnemyVisionSensor(transform);
        movementMotor = new EnemyMovementMotor(transform, navMeshAgent, aiModel);
        attackSystem = new EnemyAttackSystem(transform, aiModel, CreateProjectile);
        enemyBrain = new EnemyBrain(aiModel, attackSystem);

        movementMotor.ConfigurePhysics(body, movementConfig.ConfigureRigidbodyForNavMesh);

        if (autoFindPlayer)
        {
            Transform resolvedTarget = targetProvider.Resolve(target, true);
            if (resolvedTarget != null)
            {
                SetTarget(resolvedTarget);
            }
        }
    }

    private void Update()
    {
        if (!EnsureTarget())
        {
            StopAndIdle();
            return;
        }

        bool canSeeTarget = visionSensor.CanSee(
            target,
            visionConfig.ViewDistance,
            visionConfig.ViewAngle,
            visionConfig.EyeHeight,
            visionConfig.VisibilityBlockers);
        EnemyBrainDecision decision = enemyBrain.Evaluate(
            target,
            canSeeTarget,
            Time.time,
            attackConfig.AttackMode,
            attackConfig.MeleeAttackRange,
            attackConfig.RangedAttackRange);

        if (decision.Action == EnemyBrainAction.Idle)
        {
            StopAndIdle();
            return;
        }

        if (decision.Action == EnemyBrainAction.Chase)
        {
            movementMotor.Chase(
                decision.ChasePoint,
                movementConfig.MoveSpeed,
                movementConfig.RotationSpeed,
                Time.deltaTime);
            return;
        }

        movementMotor.Stop();
        TryAttack(decision.CanSeeTarget);
    }

    public int GetDamage()
    {
        return Mathf.Max(0, attackConfig.Damage);
    }

    private bool EnsureTarget()
    {
        if (target != null)
        {
            return true;
        }

        Transform resolvedTarget = targetProvider.Resolve(target, autoFindPlayer);
        if (resolvedTarget != null)
        {
            SetTarget(resolvedTarget);
        }

        return target != null;
    }

    private void StopAndIdle()
    {
        movementMotor.Stop();
        movementMotor.RotateIdle(
            movementConfig.PatrolLookAroundWhenIdle,
            movementConfig.IdleTurnSpeed,
            Time.deltaTime);
    }

    private void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target != null)
        {
            aiModel.RememberTarget(target.position, Time.time);
        }
    }

    private void TryAttack(bool canSeeTarget)
    {
        if (target == null || !canSeeTarget)
        {
            return;
        }

        EnemyAttackResult result = attackSystem.TryAttack(
            target,
            attackConfig.AttackMode,
            GetDamage(),
            Time.time,
            projectileConfig.ProjectileSpeed,
            projectileConfig.ProjectileLifetime,
            projectileConfig.ProjectileRadius,
            projectileConfig.ProjectileSpawnPoint,
            projectileConfig.ProjectileSpawnHeightOffset,
            projectileConfig.ProjectileSpawnForwardOffset,
            projectileConfig.ProjectileHitMask,
            enableCombatDebugLogs,
            out float cooldownRemaining);

        HandleAttackResult(result, cooldownRemaining);
    }

    private void HandleAttackResult(EnemyAttackResult result, float cooldownRemaining)
    {
        switch (result)
        {
            case EnemyAttackResult.Cooldown:
                if (logCooldownBlocks)
                {
                    Log($"Attack blocked by cooldown: {cooldownRemaining:0.00}s left.");
                }
                break;

            case EnemyAttackResult.MeleeHit:
                MeleeAttackPerformed?.Invoke();
                Log($"Attack hit player: {target.name}, dmg={GetDamage()}");
                break;

            case EnemyAttackResult.MeleeMiss:
                MeleeAttackPerformed?.Invoke();
                Log("Attack attempted, but no damage receiver found on target.");
                break;

            case EnemyAttackResult.RangedFired:
                RangedAttackPerformed?.Invoke();
                Log($"Ranged attack fired at: {target.name}");
                break;

            case EnemyAttackResult.RangedBlocked:
                if (logCooldownBlocks)
                {
                    Log("Ranged attack blocked (missing projectile target or factory failed).");
                }
                break;
        }
    }

    private EnemyProjectile CreateProjectile(string _)
    {
        return projectileFactory.CreateProjectile();
    }

    private void ApplyDefaultConfigsIfNeeded()
    {
        if (visionConfig.ViewDistance <= 0f)
        {
            visionConfig = EnemyVisionConfig.CreateDefault();
        }

        if (movementConfig.MoveSpeed <= 0f)
        {
            movementConfig = EnemyMovementConfig.CreateDefault();
        }

        if (attackConfig.AttackCooldown <= 0f)
        {
            attackConfig = EnemyAttackConfig.CreateDefault();
        }

        if (projectileConfig.ProjectileLifetime <= 0f)
        {
            projectileConfig = EnemyProjectileConfig.CreateDefault();
        }
    }

    private void Log(string message)
    {
        if (!enableCombatDebugLogs)
        {
            return;
        }

        Debug.Log($"[EnemyController] {message}", this);
    }
}
