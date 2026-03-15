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

    private IDamageSource damageSource;
    private NavMeshAgent navMeshAgent;
    private Rigidbody body;
    private float nextAttackTime;

    private void Awake()
    {
        damageSource = GetComponent<IDamageSource>();
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

        if (!CanSeeTarget())
        {
            StopChasing();
            RotateIdle();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > attackRange)
        {
            ChaseTarget();
            return;
        }

        StopChasing();
        TryAttack();
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
            return;
        }

        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            target = playerMovement.transform;
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

    private void ChaseTarget()
    {
        if (UseNavMeshAgent())
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(target.position);
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector3 direction = toTarget.normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void StopChasing()
    {
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

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        int damage = damageSource != null ? damageSource.GetDamage() : fallbackDamage;
        ApplyDamageToTarget(damage);
    }

    private void ApplyDamageToTarget(int damage)
    {
        if (target == null || damage <= 0)
        {
            return;
        }

        if (TryGetDamageReceiver(target, out IDamage directDamage, out IDamageable mixedDamage))
        {
            if (directDamage != null)
            {
                directDamage.TakeDamage(damage);
                return;
            }

            mixedDamage.TakeDamage(damage, 0f);
        }
    }

    private static bool TryGetDamageReceiver(Transform targetTransform, out IDamage damage, out IDamageable damageable)
    {
        damage = targetTransform.GetComponent<IDamage>();
        if (damage == null)
        {
            damage = targetTransform.GetComponentInParent<IDamage>();
        }

        if (damage == null)
        {
            damage = targetTransform.GetComponentInChildren<IDamage>();
        }

        damageable = targetTransform.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = targetTransform.GetComponentInParent<IDamageable>();
        }

        if (damageable == null)
        {
            damageable = targetTransform.GetComponentInChildren<IDamageable>();
        }

        return damage != null || damageable != null;
    }
}
