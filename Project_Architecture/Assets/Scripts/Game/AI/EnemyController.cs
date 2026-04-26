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
    [SerializeField] private float viewDistance = 12f;
    [SerializeField, Range(1f, 360f)] private float viewAngle = 120f;
    [SerializeField] private float eyeHeight = 1.4f;
    [SerializeField] private float visibilityMemoryDuration = 1.5f;
    [SerializeField] private LayerMask visibilityBlockers = ~0;
    [SerializeField] private bool patrolLookAroundWhenIdle = true;

    [Header("Chase")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float idleTurnSpeed = 45f;

    [Header("Attack")]
    [SerializeField] private EnemyAttackKind attackMode = EnemyAttackKind.Melee;
    [SerializeField] private int damage = 10;
    [SerializeField] private float meleeAttackRange = 1.8f;
    [SerializeField] private float rangedAttackRange = 10f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 14f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private float projectileRadius = 0.2f;
    [SerializeField] private float projectileSpawnHeightOffset = 1.2f;
    [SerializeField] private float projectileSpawnForwardOffset = 0.4f;
    [SerializeField] private LayerMask projectileHitMask = ~0;

    [Header("Physics")]
    [SerializeField] private bool configureRigidbodyForNavMesh = true;

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

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        aiModel = new EnemyAiModel(visibilityMemoryDuration, attackCooldown);
        visionSensor = new EnemyVisionSensor(transform);
        movementMotor = new EnemyMovementMotor(transform, navMeshAgent, aiModel);
        attackSystem = new EnemyAttackSystem(transform, aiModel, CreateProjectile);

        movementMotor.ConfigurePhysics(body, configureRigidbodyForNavMesh);

        if (autoFindPlayer)
        {
            TryFindPlayer();
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
            viewDistance,
            viewAngle,
            eyeHeight,
            visibilityBlockers);

        if (canSeeTarget)
        {
            aiModel.RememberTarget(target.position, Time.time);
        }

        if (!canSeeTarget && !aiModel.HasMemory(Time.time))
        {
            StopAndIdle();
            return;
        }

        if (!canSeeTarget || !attackSystem.IsInRange(target, attackMode, meleeAttackRange, rangedAttackRange))
        {
            Vector3 chasePoint = aiModel.GetChasePoint(target.position, canSeeTarget);
            movementMotor.Chase(chasePoint, moveSpeed, rotationSpeed, Time.deltaTime);
            return;
        }

        movementMotor.Stop();
        TryAttack(canSeeTarget);
    }

    public int GetDamage()
    {
        return Mathf.Max(0, damage);
    }

    private bool EnsureTarget()
    {
        if (target != null)
        {
            return true;
        }

        if (autoFindPlayer)
        {
            TryFindPlayer();
        }

        return target != null;
    }

    private void StopAndIdle()
    {
        movementMotor.Stop();
        movementMotor.RotateIdle(patrolLookAroundWhenIdle, idleTurnSpeed, Time.deltaTime);
    }

    private void TryFindPlayer()
    {
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag(playerTag);
        if (taggedPlayer != null)
        {
            SetTarget(taggedPlayer.transform);
            return;
        }

        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            SetTarget(playerMovement.transform);
        }
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
            attackMode,
            GetDamage(),
            Time.time,
            projectileSpeed,
            projectileLifetime,
            projectileRadius,
            projectileSpawnPoint,
            projectileSpawnHeightOffset,
            projectileSpawnForwardOffset,
            projectileHitMask,
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

    private EnemyProjectile CreateProjectile(string fallbackName)
    {
        if (projectilePrefab != null)
        {
            GameObject projectileObject = Instantiate(projectilePrefab);
            if (projectileObject.TryGetComponent(out EnemyProjectile projectile))
            {
                return projectile;
            }

            Log("Projectile prefab has no EnemyProjectile component. Added at runtime.");
            return projectileObject.AddComponent<EnemyProjectile>();
        }

        GameObject fallbackProjectileObject = new GameObject(fallbackName);
        Log("Projectile prefab is not assigned. Spawned runtime projectile.");
        return fallbackProjectileObject.AddComponent<EnemyProjectile>();
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
