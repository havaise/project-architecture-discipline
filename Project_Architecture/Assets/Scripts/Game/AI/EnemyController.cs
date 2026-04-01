using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour, IMovementStateProvider, IEnemyAttackEvents, IDamageSource
{
    private enum AttackMode
    {
        Melee = 0,
        Ranged = 1
    }

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
    [SerializeField] private AttackMode attackMode = AttackMode.Melee;
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

    public bool IsMoving => isMoving;
    public float MoveSpeedNormalized => moveSpeedNormalized;

    private NavMeshAgent navMeshAgent;
    private Rigidbody body;
    private float nextAttackTime;
    private float visibleUntilTime;
    private Vector3 lastKnownTargetPosition;
    private bool isMoving;
    private float moveSpeedNormalized;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();

        ConfigurePhysics();

        if (autoFindPlayer)
        {
            TryFindPlayer();
        }
    }

    private void Update()
    {
        if (target == null)
        {
            if (autoFindPlayer)
            {
                TryFindPlayer();
            }

            StopChasing();
            RotateIdle();
            return;
        }

        bool canSeeTarget = CanSeeTarget();
        if (canSeeTarget)
        {
            visibleUntilTime = Time.time + visibilityMemoryDuration;
            lastKnownTargetPosition = target.position;
        }

        bool targetRemembered = Time.time <= visibleUntilTime;
        if (!canSeeTarget && !targetRemembered)
        {
            StopChasing();
            RotateIdle();
            return;
        }

        if (!canSeeTarget || !IsTargetInAttackRange())
        {
            Vector3 chasePoint = canSeeTarget ? target.position : lastKnownTargetPosition;
            ChaseTarget(chasePoint);
            return;
        }

        StopChasing();
        TryAttack(canSeeTarget);
    }

    public int GetDamage()
    {
        return Mathf.Max(0, damage);
    }

    private void ConfigurePhysics()
    {
        if (!configureRigidbodyForNavMesh)
        {
            return;
        }

        if (body == null || navMeshAgent == null)
        {
            return;
        }

        body.useGravity = false;
        body.isKinematic = true;
        body.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void RotateIdle()
    {
        if (!patrolLookAroundWhenIdle)
        {
            return;
        }

        transform.Rotate(Vector3.up, idleTurnSpeed * Time.deltaTime, Space.World);
    }

    private void TryFindPlayer()
    {
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag(playerTag);
        if (taggedPlayer != null)
        {
            target = taggedPlayer.transform;
            lastKnownTargetPosition = target.position;
            visibleUntilTime = Time.time;
            return;
        }

        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            target = playerMovement.transform;
            lastKnownTargetPosition = target.position;
            visibleUntilTime = Time.time;
        }
    }

    private bool CanSeeTarget()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPosition = target.position + Vector3.up * eyeHeight;
        Vector3 toTarget = targetPosition - origin;

        if (toTarget.sqrMagnitude > viewDistance * viewDistance)
        {
            return false;
        }

        float angle = Vector3.Angle(transform.forward, toTarget);
        if (angle > viewAngle * 0.5f)
        {
            return false;
        }

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            toTarget.normalized,
            viewDistance,
            visibilityBlockers,
            QueryTriggerInteraction.Ignore);

        if (hits.Length == 0)
        {
            return true;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.transform;

            if (hitTransform == transform || hitTransform.IsChildOf(transform))
            {
                continue;
            }

            return hitTransform == target || hitTransform.IsChildOf(target);
        }

        return true;
    }

    private bool IsTargetInAttackRange()
    {
        if (target == null)
        {
            return false;
        }

        float range = attackMode == AttackMode.Ranged ? rangedAttackRange : meleeAttackRange;

        return Vector3.Distance(transform.position, target.position) <= range;
    }

    private void ChaseTarget(Vector3 destination)
    {
        if (UseNavMeshAgent())
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(destination);

            float effectiveSpeed = Mathf.Max(navMeshAgent.velocity.magnitude, navMeshAgent.desiredVelocity.magnitude);
            float speed01 = navMeshAgent.speed > 0.01f
                ? Mathf.Clamp01(effectiveSpeed / navMeshAgent.speed)
                : 0f;

            if (speed01 <= 0.01f && Vector3.Distance(transform.position, destination) > 0.2f)
            {
                bool shouldMove =
                    navMeshAgent.pathPending
                    || (navMeshAgent.hasPath && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance + 0.05f)
                    || Vector3.Distance(transform.position, destination) > 0.2f;

                if (shouldMove)
                {
                    speed01 = 1f;
                }
            }

            SetMoveState(speed01 > 0.01f, speed01);
            return;
        }

        Vector3 toTarget = destination - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            SetMoveState(false, 0f);
            return;
        }

        Vector3 direction = toTarget.normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        SetMoveState(true, 1f);
    }

    private void StopChasing()
    {
        SetMoveState(false, 0f);

        if (!UseNavMeshAgent())
        {
            return;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    private bool UseNavMeshAgent()
    {
        return navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh;
    }

    private void TryAttack(bool canSeeTarget)
    {
        if (target == null || !canSeeTarget)
        {
            return;
        }

        if (Time.time < nextAttackTime)
        {
            if (logCooldownBlocks)
            {
                Log($"Attack blocked by cooldown: {(nextAttackTime - Time.time):0.00}s left.");
            }

            return;
        }

        nextAttackTime = Time.time + attackCooldown;

        if (attackMode == AttackMode.Ranged)
        {
            if (TryRangedAttack(target))
            {
                RangedAttackPerformed?.Invoke();
                Log($"Ranged attack fired at: {target.name}");
            }
            else if (logCooldownBlocks)
            {
                Log("Ranged attack blocked (range or missing target).");
            }

            return;
        }

        MeleeAttackPerformed?.Invoke();

        if (CombatDamageResolver.TryApplyDamage(target, GetDamage(), 0f))
        {
            Log($"Attack hit player: {target.name}, dmg={GetDamage()}");
            return;
        }

        Log("Attack attempted, but no damage receiver found on target.");
    }

    private bool TryRangedAttack(Transform attackTarget)
    {
        if (attackTarget == null)
        {
            return false;
        }

        Vector3 spawnPosition = GetProjectileSpawnPosition();
        Vector3 targetPoint = attackTarget.position + Vector3.up * projectileSpawnHeightOffset;
        Vector3 direction = targetPoint - spawnPosition;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = transform.forward;
        }

        direction.Normalize();

        EnemyProjectile projectile = SpawnProjectile(spawnPosition, direction);
        projectile.Initialize(
            transform,
            direction,
            GetDamage(),
            projectileSpeed,
            projectileLifetime,
            projectileRadius,
            projectileHitMask,
            enableCombatDebugLogs);

        return true;
    }

    private EnemyProjectile SpawnProjectile(Vector3 spawnPosition, Vector3 direction)
    {
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

        if (projectilePrefab != null)
        {
            GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, rotation);
            if (projectileObject.TryGetComponent(out EnemyProjectile projectile))
            {
                return projectile;
            }

            Log("Projectile prefab has no EnemyProjectile component. Added at runtime.");
            return projectileObject.AddComponent<EnemyProjectile>();
        }

        GameObject fallbackProjectileObject = new GameObject("EnemyProjectile");
        fallbackProjectileObject.transform.SetPositionAndRotation(spawnPosition, rotation);
        Log("Projectile prefab is not assigned. Spawned runtime projectile.");
        return fallbackProjectileObject.AddComponent<EnemyProjectile>();
    }

    private Vector3 GetProjectileSpawnPosition()
    {
        if (projectileSpawnPoint != null)
        {
            return projectileSpawnPoint.position;
        }

        return transform.position
            + Vector3.up * projectileSpawnHeightOffset
            + transform.forward * projectileSpawnForwardOffset;
    }

    private void SetMoveState(bool moving, float speed01)
    {
        isMoving = moving;
        moveSpeedNormalized = Mathf.Clamp01(speed01);
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
