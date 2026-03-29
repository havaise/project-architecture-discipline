using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
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
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private int fallbackDamage = 10;

    [Header("Physics")]
    [SerializeField] private bool configureRigidbodyForNavMesh = true;

    [Header("Debug")]
    [SerializeField] private bool enableCombatDebugLogs;
    [SerializeField] private bool logCooldownBlocks;

    public event Action MeleeAttackPerformed;
    public event Action RangedAttackPerformed;

    public bool IsMoving => isMoving;
    public float MoveSpeedNormalized => moveSpeedNormalized;

    private IDamageSource damageSource;
    private RangedMob rangedMob;
    private NavMeshAgent navMeshAgent;
    private Rigidbody body;
    private float nextAttackTime;
    private float visibleUntilTime;
    private Vector3 lastKnownTargetPosition;
    private bool isMoving;
    private float moveSpeedNormalized;

    private void Awake()
    {
        damageSource = GetComponent<IDamageSource>();
        rangedMob = GetComponent<RangedMob>();
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

        if (rangedMob != null)
        {
            return rangedMob.CanAttack(target);
        }

        return Vector3.Distance(transform.position, target.position) <= attackRange;
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

        if (rangedMob != null)
        {
            bool projectileFired = rangedMob.TryAttack(target);
            if (projectileFired)
            {
                RangedAttackPerformed?.Invoke();
                Log($"Ranged attack fired at: {target.name}");
            }
            else if (logCooldownBlocks)
            {
                Log("Ranged attack blocked (cooldown, range, or missing projectile prefab).");
            }

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
        MeleeAttackPerformed?.Invoke();

        float damage = damageSource != null ? damageSource.GetDamage() : fallbackDamage;

        if (CombatDamageResolver.TryApplyDamage(target, damage, 0f))
        {
            Log($"Attack hit player: {target.name}, dmg={damage:0.#}");
            return;
        }

        Log("Attack attempted, but no damage receiver found on target.");
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

        Debug.Log($"[EnemyAI] {message}", this);
    }
}

