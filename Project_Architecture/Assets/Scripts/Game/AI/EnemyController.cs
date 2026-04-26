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

    public bool IsMoving => enemyAgent != null && enemyAgent.IsMoving;
    public float MoveSpeedNormalized => enemyAgent != null ? enemyAgent.MoveSpeedNormalized : 0f;

    private NavMeshAgent navMeshAgent;
    private Rigidbody body;
    private EnemyAgent enemyAgent;
    private IEnemyProjectileFactory projectileFactory;

    private void Awake()
    {
        ApplyDefaultConfigsIfNeeded();

        navMeshAgent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        IEnemyTargetProvider targetProvider = new UnityEnemyTargetProvider(playerTag);
        projectileFactory = new UnityEnemyProjectileFactory(projectileConfig.ProjectilePrefab, Log);
        EnemyAiModel aiModel = new EnemyAiModel(visionConfig.VisibilityMemoryDuration, attackConfig.AttackCooldown);
        EnemyVisionSensor visionSensor = new EnemyVisionSensor(transform);
        EnemyMovementMotor movementMotor = new EnemyMovementMotor(transform, navMeshAgent, aiModel);
        EnemyAttackSystem attackSystem = new EnemyAttackSystem(transform, aiModel, CreateProjectile);

        movementMotor.ConfigurePhysics(body, movementConfig.ConfigureRigidbodyForNavMesh);

        enemyAgent = new EnemyAgent(
            target,
            aiModel,
            visionSensor,
            movementMotor,
            attackSystem,
            targetProvider,
            visionConfig,
            movementConfig,
            attackConfig,
            projectileConfig,
            autoFindPlayer,
            GetDamage,
            HandleAttackResult,
            enableCombatDebugLogs);
        enemyAgent.Initialize(Time.time);
        target = enemyAgent.CurrentTarget;
    }

    private void Update()
    {
        if (enemyAgent == null)
        {
            return;
        }

        enemyAgent.Tick(Time.time, Time.deltaTime);
        target = enemyAgent.CurrentTarget;
    }

    public int GetDamage()
    {
        return Mathf.Max(0, attackConfig.Damage);
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
