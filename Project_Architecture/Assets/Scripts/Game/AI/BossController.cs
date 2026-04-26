using System;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour, IMovementStateProvider, IEnemyAttackEvents, IDamageSource
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

    [Header("Boss")]
    [SerializeField] private bool requireHitToAggro = true;
    [SerializeField] private BossCombatConfig bossCombatConfig = default;

    [Header("Debug")]
    [SerializeField] private bool enableCombatDebugLogs;
    [SerializeField] private bool logCooldownBlocks;

    public event Action MeleeAttackPerformed;
    public event Action RangedAttackPerformed;

    public bool IsMoving => bossAgent != null && bossAgent.IsMoving;
    public float MoveSpeedNormalized => bossAgent != null ? bossAgent.MoveSpeedNormalized : 0f;

    private NavMeshAgent navMeshAgent;
    private Rigidbody body;
    private BossAgent bossAgent;
    private IEnemyProjectileFactory projectileFactory;
    private HealthComponent healthComponent;
    private int lastKnownHealth = int.MaxValue;
    private bool wasHit;

    private void Awake()
    {
        ApplyDefaultConfigsIfNeeded();

        navMeshAgent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        healthComponent = GetComponentInChildren<HealthComponent>();

        IEnemyTargetProvider targetProvider = new UnityEnemyTargetProvider(playerTag);
        projectileFactory = new UnityEnemyProjectileFactory(projectileConfig.ProjectilePrefab, Log);
        EnemyAiModel aiModel = new EnemyAiModel(visionConfig.VisibilityMemoryDuration, attackConfig.AttackCooldown);
        EnemyVisionSensor visionSensor = new EnemyVisionSensor(transform);
        EnemyMovementMotor movementMotor = new EnemyMovementMotor(transform, navMeshAgent, aiModel);
        EnemyAttackSystem attackSystem = new EnemyAttackSystem(transform, aiModel, CreateProjectile);
        movementMotor.ConfigurePhysics(body, movementConfig.ConfigureRigidbodyForNavMesh);

        bossAgent = new BossAgent(
            transform,
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
            bossCombatConfig,
            autoFindPlayer,
            GetDamage,
            GetHealthRatio,
            HandleAttackResult,
            enableCombatDebugLogs);
        bossAgent.Initialize(Time.time);
        target = bossAgent.CurrentTarget;
    }

    private void Start()
    {
        if (healthComponent != null)
        {
            lastKnownHealth = healthComponent.Current;
            healthComponent.HealthChanged += OnHealthChanged;
        }
    }

    private void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.HealthChanged -= OnHealthChanged;
        }
    }

    private void Update()
    {
        if (bossAgent == null)
        {
            return;
        }

        bool provoked = !requireHitToAggro || wasHit;
        bossAgent.SetProvoked(provoked);
        bossAgent.Tick(Time.time, Time.deltaTime);
        target = bossAgent.CurrentTarget;
    }

    public int GetDamage()
    {
        return Mathf.Max(0, attackConfig.Damage);
    }

    private float GetHealthRatio()
    {
        if (healthComponent == null || healthComponent.Max <= 0)
        {
            return 1f;
        }

        return (float)healthComponent.Current / healthComponent.Max;
    }

    private void OnHealthChanged(int current, int max)
    {
        if (lastKnownHealth != int.MaxValue && current < lastKnownHealth)
        {
            wasHit = true;
        }

        lastKnownHealth = current;
    }

    private void HandleAttackResult(EnemyAttackResult result, float cooldownRemaining)
    {
        switch (result)
        {
            case EnemyAttackResult.Cooldown:
                if (logCooldownBlocks)
                {
                    Log($"Boss attack blocked by cooldown: {cooldownRemaining:0.00}s left.");
                }
                break;

            case EnemyAttackResult.MeleeHit:
            case EnemyAttackResult.MeleeMiss:
                MeleeAttackPerformed?.Invoke();
                break;

            case EnemyAttackResult.RangedFired:
                RangedAttackPerformed?.Invoke();
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

        if (bossCombatConfig.StrongAttackCooldown <= 0f)
        {
            bossCombatConfig = BossCombatConfig.CreateDefault();
        }
    }

    private void Log(string message)
    {
        if (!enableCombatDebugLogs)
        {
            return;
        }

        Debug.Log($"[BossController] {message}", this);
    }
}
