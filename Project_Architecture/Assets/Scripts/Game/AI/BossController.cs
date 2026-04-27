using System;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour, IMovementStateProvider, IEnemyAttackEvents, IDamageSource, IStateNameProvider
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
    [SerializeField] private bool allowDeaggro = true;
    [SerializeField] private float deaggroDistance = 28f;
    [SerializeField] private float deaggroDelay = 3f;

    [Header("UI")]
    [SerializeField] private HudView worldHudView;

    [Header("Debug")]
    [SerializeField] private bool enableCombatDebugLogs;
    [SerializeField] private bool logCooldownBlocks;

    public event Action MeleeAttackPerformed;
    public event Action RangedAttackPerformed;

    public bool IsMoving => bossAgent != null && bossAgent.IsMoving;
    public float MoveSpeedNormalized => bossAgent != null ? bossAgent.MoveSpeedNormalized : 0f;
    public string CurrentStateName => bossAgent != null ? bossAgent.CurrentStateName : string.Empty;

    private NavMeshAgent navMeshAgent;
    private Rigidbody body;
    private BossAgent bossAgent;
    private IEnemyProjectileFactory projectileFactory;
    private HealthComponent healthComponent;
    private int lastKnownHealth = int.MaxValue;
    private bool wasHit;
    private float outOfRangeTime;

    private void Awake()
    {
        ApplyDefaultConfigsIfNeeded();

        navMeshAgent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        healthComponent = GetComponentInChildren<HealthComponent>();
        if (worldHudView == null)
        {
            worldHudView = GetComponentInChildren<HudView>(true);
        }

        if (worldHudView != null)
        {
            worldHudView.Initialize(healthComponent, null);
        }

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
        UpdateDeaggro(Time.deltaTime);
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
            outOfRangeTime = 0f;
        }

        lastKnownHealth = current;
    }

    private void UpdateDeaggro(float deltaTime)
    {
        if (!allowDeaggro || !requireHitToAggro || !wasHit)
        {
            outOfRangeTime = 0f;
            return;
        }

        float safeDistance = Mathf.Max(1f, deaggroDistance);
        bool outOfRange = target == null || Vector3.Distance(transform.position, target.position) > safeDistance;
        if (!outOfRange)
        {
            outOfRangeTime = 0f;
            return;
        }

        outOfRangeTime += Mathf.Max(0f, deltaTime);
        if (outOfRangeTime < Mathf.Max(0.1f, deaggroDelay))
        {
            return;
        }

        wasHit = false;
        outOfRangeTime = 0f;

        if (enableCombatDebugLogs)
        {
            Debug.Log("[BossController] De-aggro activated: target is out of range for too long.", this);
        }
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

        deaggroDistance = Mathf.Max(1f, deaggroDistance);
        deaggroDelay = Mathf.Max(0.1f, deaggroDelay);
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
