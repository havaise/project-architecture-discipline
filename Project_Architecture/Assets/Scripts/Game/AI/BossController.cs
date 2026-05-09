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

    [Header("Boss Loadout")]
    [SerializeField] private bool randomizeOnSpawn = true;
    [SerializeField] private BossAttackType currentAttackType = BossAttackType.Melee;
    [SerializeField] private BossElement currentElement = BossElement.Fire;
    [SerializeField] private BossElementAttackProfile[] elementProfiles;
    [SerializeField] private AudioSource audioSource;

    [Header("Boss")]
    [SerializeField] private bool requireHitToAggro = true;
    [SerializeField] private BossCombatConfig bossCombatConfig = default;
    [SerializeField] private bool allowDeaggro = true;
    [SerializeField] private float deaggroDistance = 28f;
    [SerializeField] private float deaggroDelay = 3f;
    [SerializeField] private bool canChangeElementDuringFight;
    [SerializeField] private float elementChangeInterval = 20f;
    [SerializeField] private bool canChangeAttackTypeDuringFight;
    [SerializeField] private float attackTypeChangeInterval = 30f;

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
    private float nextElementChangeTime;
    private float nextAttackTypeChangeTime;
    private float damageMultiplier = 1f;
    private Color projectileTint = Color.white;
    private GameObject projectileHitVfx;
    private GameObject attackVfx;
    private AudioClip attackSfx;
    private EnemyAttackConfig baseAttackConfig;
    private EnemyProjectileConfig baseProjectileConfig;

    private void Awake()
    {
        ApplyDefaultConfigsIfNeeded();
        baseAttackConfig = attackConfig;
        baseProjectileConfig = projectileConfig;
        RandomizeBossLoadout();

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
            projectileTint,
            projectileHitVfx,
            enableCombatDebugLogs);
        bossAgent.Initialize(Time.time);
        target = bossAgent.CurrentTarget;
        nextElementChangeTime = Time.time + Mathf.Max(1f, elementChangeInterval);
        nextAttackTypeChangeTime = Time.time + Mathf.Max(1f, attackTypeChangeInterval);
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
        UpdateDynamicLoadout(Time.time);
        bossAgent.Tick(Time.time, Time.deltaTime);
        target = bossAgent.CurrentTarget;
        UpdateDeaggro(Time.deltaTime);
    }

    public int GetDamage()
    {
        return Mathf.Max(0, Mathf.RoundToInt(attackConfig.Damage * Mathf.Max(0.1f, damageMultiplier)));
    }

    [ContextMenu("Randomize Boss Loadout")]
    public void RandomizeBossLoadout()
    {
        if (randomizeOnSpawn)
        {
            currentAttackType = UnityEngine.Random.value < 0.5f ? BossAttackType.Melee : BossAttackType.Ranged;
            currentElement = (BossElement)UnityEngine.Random.Range(0, 4);
        }

        ApplyBossLoadout();
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
                PlayPresentation();
                break;

            case EnemyAttackResult.RangedFired:
                RangedAttackPerformed?.Invoke();
                PlayPresentation();
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
        else
        {
            if (movementConfig.NavAcceleration <= 0f)
            {
                movementConfig.NavAcceleration = 16f;
            }

            if (movementConfig.NavAngularSpeed <= 0f)
            {
                movementConfig.NavAngularSpeed = 540f;
            }

            if (movementConfig.NavStoppingDistance < 0f)
            {
                movementConfig.NavStoppingDistance = 0.8f;
            }

            if (movementConfig.NavPathRepathInterval <= 0f)
            {
                movementConfig.NavPathRepathInterval = 0.2f;
            }
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

    private void ApplyBossLoadout()
    {
        attackConfig = baseAttackConfig;
        projectileConfig = baseProjectileConfig;
        attackConfig.AttackMode = currentAttackType == BossAttackType.Melee ? EnemyAttackKind.Melee : EnemyAttackKind.Ranged;
        BossElementAttackProfile profile = FindProfile(currentAttackType, currentElement);
        damageMultiplier = profile.DamageMultiplier <= 0f ? 1f : profile.DamageMultiplier;
        float cooldownMul = profile.CooldownMultiplier <= 0f ? 1f : profile.CooldownMultiplier;
        attackConfig.AttackCooldown = Mathf.Max(0.05f, attackConfig.AttackCooldown * cooldownMul);
        projectileConfig.ProjectileSpeed = Mathf.Max(0.01f, projectileConfig.ProjectileSpeed * Mathf.Max(0.05f, profile.ProjectileSpeedMultiplier));
        projectileTint = profile.Color.a <= 0f ? Color.white : profile.Color;
        projectileHitVfx = profile.HitVfxPrefab;
        attackVfx = profile.AttackVfxPrefab;
        attackSfx = profile.Sound;
    }

    private BossElementAttackProfile FindProfile(BossAttackType attackType, BossElement element)
    {
        if (elementProfiles != null)
        {
            for (int i = 0; i < elementProfiles.Length; i++)
            {
                if (elementProfiles[i].AttackType == attackType && elementProfiles[i].Element == element)
                {
                    return elementProfiles[i];
                }
            }
        }

        return new BossElementAttackProfile
        {
            AttackType = attackType,
            Element = element,
            DamageMultiplier = 1f,
            CooldownMultiplier = 1f,
            ProjectileSpeedMultiplier = 1f,
            Color = Color.white
        };
    }

    private void UpdateDynamicLoadout(float currentTime)
    {
        if (canChangeElementDuringFight && currentTime >= nextElementChangeTime)
        {
            currentElement = (BossElement)UnityEngine.Random.Range(0, 4);
            ApplyBossLoadout();
            nextElementChangeTime = currentTime + Mathf.Max(1f, elementChangeInterval);
        }

        if (canChangeAttackTypeDuringFight && currentTime >= nextAttackTypeChangeTime)
        {
            currentAttackType = currentAttackType == BossAttackType.Melee ? BossAttackType.Ranged : BossAttackType.Melee;
            ApplyBossLoadout();
            nextAttackTypeChangeTime = currentTime + Mathf.Max(1f, attackTypeChangeInterval);
        }
    }

    private void PlayPresentation()
    {
        if (attackVfx != null)
        {
            Instantiate(attackVfx, transform.position + Vector3.up, Quaternion.identity);
        }

        if (attackSfx == null)
        {
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(attackSfx);
            return;
        }

        AudioSource.PlayClipAtPoint(attackSfx, transform.position);
    }
}
