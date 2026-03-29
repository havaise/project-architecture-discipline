using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Fallback Settings")]
    [SerializeField] private int fallbackDamage = 7;
    [SerializeField] private float fallbackSpeed = 14f;
    [SerializeField] private float fallbackLifetime = 3f;
    [SerializeField] private float fallbackRadius = 0.2f;

    private Transform owner;
    private Vector3 moveDirection;
    private float speed;
    private float lifeTime;
    private float hitRadius;
    private int damage;
    private LayerMask targetMask;
    private bool debugLogs;

    private float aliveTime;
    private Vector3 previousPosition;
    private bool initialized;

    public void Initialize(
        Transform ownerTransform,
        Vector3 direction,
        int projectileDamage,
        float moveSpeed,
        float lifetime,
        float radius,
        LayerMask mask,
        bool enableDebug)
    {
        owner = ownerTransform;
        moveDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
        damage = Mathf.Max(0, projectileDamage);
        speed = Mathf.Max(0f, moveSpeed);
        lifeTime = Mathf.Max(0.01f, lifetime);
        hitRadius = Mathf.Max(0.01f, radius);
        targetMask = mask;
        debugLogs = enableDebug;

        previousPosition = transform.position;
        aliveTime = 0f;
        initialized = true;
    }

    private void Awake()
    {
        if (initialized)
        {
            return;
        }

        owner = null;
        moveDirection = transform.forward;
        damage = fallbackDamage;
        speed = fallbackSpeed;
        lifeTime = fallbackLifetime;
        hitRadius = fallbackRadius;
        targetMask = ~0;
        debugLogs = false;
        previousPosition = transform.position;
        aliveTime = 0f;
        initialized = true;
    }

    private void Update()
    {
        aliveTime += Time.deltaTime;
        if (aliveTime >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 position = previousPosition + moveDirection * (speed * Time.deltaTime);
        Vector3 delta = position - previousPosition;
        float distance = delta.magnitude;

        if (distance > 0.0001f)
        {
            Vector3 direction = delta / distance;
            if (Physics.SphereCast(previousPosition, hitRadius, direction, out RaycastHit hit, distance, targetMask, QueryTriggerInteraction.Ignore))
            {
                if (!IsOwner(hit.transform) && CombatDamageResolver.TryApplyDamage(hit.transform, damage, 0f))
                {
                    Log($"Projectile hit: {hit.transform.name}, dmg={damage}");
                    Destroy(gameObject);
                    return;
                }
            }
        }

        transform.position = position;
        transform.forward = moveDirection;
        previousPosition = position;
    }

    private bool IsOwner(Transform hitTransform)
    {
        if (owner == null || hitTransform == null)
        {
            return false;
        }

        return hitTransform == owner || hitTransform.IsChildOf(owner) || owner.IsChildOf(hitTransform);
    }

    private void Log(string message)
    {
        if (!debugLogs)
        {
            return;
        }

        Debug.Log($"[EnemyProjectile] {message}", this);
    }
}
