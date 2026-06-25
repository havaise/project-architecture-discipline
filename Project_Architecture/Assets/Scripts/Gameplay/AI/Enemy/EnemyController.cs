using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour, IMovementStateProvider, IEnemyAttackEvents, IDamageSource, IStateNameProvider
{
    public static event Action<EnemyController> EnemyDied;

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

    [Header("Weapons")]
    [SerializeField] private MobWeaponConfig defaultWeapon;
    [SerializeField] private MobWeaponConfig[] availableWeapons;
    [SerializeField] private bool randomizeWeaponOnSpawn = true;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform meleeVfxSpawnPoint;

    [Header("Behaviour")]
    [SerializeField] private EnemyBehaviourConfig behaviourConfig = default;

    [Header("UI")]
    [SerializeField] private HudView worldHudView;

    [Header("Debug")]
    [SerializeField] private bool enableCombatDebugLogs;
    [SerializeField] private bool logCooldownBlocks;

    public event Action MeleeAttackPerformed;
    public event Action RangedAttackPerformed;

    public bool IsMoving => enemyAgent != null && enemyAgent.IsMoving;
    public float MoveSpeedNormalized => enemyAgent != null ? enemyAgent.MoveSpeedNormalized : 0f;
    public string CurrentStateName => enemyAgent != null ? enemyAgent.CurrentStateName : string.Empty;

    private NavMeshAgent navMeshAgent;
    private Rigidbody body;
    private EnemyAgent enemyAgent;
    private EnemyAgentFactory agentFactory;
    private EnemyConfigInitializer configInitializer;
    private HealthComponent healthComponent;
    private MobWeaponFactory weaponFactory;
    private MobWeaponRuntime runtimeWeapon;
    private float damageMultiplier = 1f;
    private bool deathReported;

    private void Awake()
    {
        configInitializer = new EnemyConfigInitializer();
        configInitializer.ApplyDefaults(
            ref visionConfig,
            ref movementConfig,
            ref attackConfig,
            ref projectileConfig,
            ref behaviourConfig);

        navMeshAgent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        healthComponent = GetComponentInChildren<HealthComponent>();
        weaponFactory = new MobWeaponFactory();
        agentFactory = new EnemyAgentFactory();
        if (worldHudView == null)
        {
            worldHudView = GetComponentInChildren<HudView>(true);
        }

        if (worldHudView != null)
        {
            worldHudView.Initialize(healthComponent, null);
        }

        if (healthComponent != null)
        {
            healthComponent.Died += OnDied;
        }
    }

    private void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.Died -= OnDied;
        }
    }

    private void Start()
    {
        runtimeWeapon = weaponFactory.Create(defaultWeapon, availableWeapons, randomizeWeaponOnSpawn);
        ApplyWeapon(runtimeWeapon);

        enemyAgent = agentFactory.Create(new EnemyAgentFactoryContext
        {
            Owner = transform,
            InitialTarget = target,
            NavMeshAgent = navMeshAgent,
            Body = body,
            PlayerTag = playerTag,
            AutoFindPlayer = autoFindPlayer,
            VisionConfig = visionConfig,
            MovementConfig = movementConfig,
            AttackConfig = attackConfig,
            ProjectileConfig = projectileConfig,
            BehaviourConfig = behaviourConfig,
            GetDamage = GetDamage,
            GetHealthRatio = GetHealthRatio,
            HandleAttackResult = HandleAttackResult,
            Log = Log,
            EnableCombatDebugLogs = enableCombatDebugLogs
        });
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
        return Mathf.Max(0, Mathf.RoundToInt(attackConfig.Damage * Mathf.Max(0.1f, damageMultiplier)));
    }

    public void SetSpawnWeaponOptions(MobWeaponConfig[] spawnWeaponOptions, bool randomize)
    {
        availableWeapons = spawnWeaponOptions;
        randomizeWeaponOnSpawn = randomize;
    }

    public void ApplySpawnMultipliers(float healthMultiplier, float damageMul, float speedMultiplier)
    {
        damageMultiplier = Mathf.Max(0.1f, damageMul);
        attackConfig.AttackCooldown = speedMultiplier > 0.01f
            ? Mathf.Max(0.05f, attackConfig.AttackCooldown / speedMultiplier)
            : attackConfig.AttackCooldown;

        if (healthComponent != null)
        {
            int boostedHealth = Mathf.Max(1, Mathf.RoundToInt(healthComponent.Max * Mathf.Max(0.1f, healthMultiplier)));
            healthComponent.SetMaxAndCurrent(boostedHealth, boostedHealth);
        }
    }

    private float GetHealthRatio()
    {
        if (healthComponent == null || healthComponent.Max <= 0)
        {
            return 1f;
        }

        return (float)healthComponent.Current / healthComponent.Max;
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
                PlayAttackPresentation();
                Log($"Attack hit player: {(target != null ? target.name : "<null>")}, dmg={GetDamage()}");
                break;

            case EnemyAttackResult.MeleeMiss:
                MeleeAttackPerformed?.Invoke();
                PlayAttackPresentation();
                Log("Attack attempted, but no damage receiver found on target.");
                break;

            case EnemyAttackResult.RangedFired:
                RangedAttackPerformed?.Invoke();
                PlayAttackPresentation();
                Log($"Ranged attack fired at: {(target != null ? target.name : "<null>")}");
                break;

            case EnemyAttackResult.RangedBlocked:
                if (logCooldownBlocks)
                {
                    Log("Ranged attack blocked (missing projectile target or factory failed).");
                }
                break;
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

    private void ApplyWeapon(MobWeaponRuntime weapon)
    {
        if (weapon == null)
        {
            return;
        }

        attackConfig.AttackMode = weapon.AttackKind;
        attackConfig.Damage = Mathf.Max(0, weapon.Damage);
        attackConfig.AttackCooldown = Mathf.Max(0.05f, weapon.AttackCooldown);
        if (weapon.AttackKind == EnemyAttackKind.Melee)
        {
            attackConfig.MeleeAttackRange = Mathf.Max(0.1f, weapon.AttackRange);
        }
        else
        {
            attackConfig.RangedAttackRange = Mathf.Max(0.1f, weapon.AttackRange);
            projectileConfig.ProjectilePrefab = weapon.ProjectilePrefab != null
                ? weapon.ProjectilePrefab
                : projectileConfig.ProjectilePrefab;
            projectileConfig.ProjectileSpeed = Mathf.Max(0.01f, weapon.ProjectileSpeed);
            projectileConfig.ProjectileLifetime = Mathf.Max(0.05f, weapon.ProjectileLifetime);
            projectileConfig.ProjectileRadius = Mathf.Max(0.01f, weapon.ProjectileRadius);
        }
    }

    private void PlayAttackPresentation()
    {
        if (runtimeWeapon == null)
        {
            return;
        }

        if (runtimeWeapon.AttackVfxPrefab != null)
        {
            Vector3 vfxPosition = transform.position + Vector3.up;
            if (runtimeWeapon.AttackKind == EnemyAttackKind.Melee && meleeVfxSpawnPoint != null)
            {
                vfxPosition = meleeVfxSpawnPoint.position;
            }

            Instantiate(runtimeWeapon.AttackVfxPrefab, vfxPosition, Quaternion.identity);
        }

        if (runtimeWeapon.AttackSfx == null)
        {
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(runtimeWeapon.AttackSfx);
            return;
        }

        AudioSource.PlayClipAtPoint(runtimeWeapon.AttackSfx, transform.position);
    }

    private void OnDied()
    {
        if (deathReported)
        {
            return;
        }

        deathReported = true;
        EnemyDied?.Invoke(this);
    }
}




