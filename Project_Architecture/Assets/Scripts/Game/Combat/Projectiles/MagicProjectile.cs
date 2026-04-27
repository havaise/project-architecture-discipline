using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    [Header("Fallback Settings")]
    [SerializeField] private float fallbackDamage = 30f;
    [SerializeField] private float fallbackSpeed = 12f;
    [SerializeField] private float fallbackLifetime = 2.5f;
    [SerializeField] private float fallbackRadius = 0.25f;
    [SerializeField] private float fallbackWaveAmplitude = 0.6f;
    [SerializeField] private float fallbackWaveFrequency = 8f;

    private Transform owner;
    private Vector3 startPosition;
    private Vector3 forwardDirection;
    private Vector3 rightDirection;
    private float damage;
    private float speed;
    private float lifeTime;
    private float hitRadius;
    private float waveAmplitude;
    private float waveFrequency;
    private LayerMask targetMask;
    private bool debugLogs;

    private float aliveTime;
    private Vector3 previousPosition;
    private bool initialized;

    public void Initialize(
        Transform ownerTransform,
        Vector3 direction,
        float magicDamage,
        float moveSpeed,
        float lifetime,
        float radius,
        float amplitude,
        float frequency,
        LayerMask mask,
        bool enableDebug)
    {
        owner = ownerTransform;
        forwardDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
        rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;
        if (rightDirection.sqrMagnitude <= 0.0001f)
        {
            rightDirection = transform.right;
        }

        damage = magicDamage;
        speed = moveSpeed;
        lifeTime = lifetime;
        hitRadius = radius;
        waveAmplitude = amplitude;
        waveFrequency = frequency;
        targetMask = mask.value == 0 ? ~0 : mask;
        debugLogs = enableDebug;

        startPosition = transform.position;
        previousPosition = startPosition;
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
        startPosition = transform.position;
        previousPosition = transform.position;
        forwardDirection = transform.forward;
        rightDirection = transform.right;
        damage = fallbackDamage;
        speed = fallbackSpeed;
        lifeTime = fallbackLifetime;
        hitRadius = fallbackRadius;
        waveAmplitude = fallbackWaveAmplitude;
        waveFrequency = fallbackWaveFrequency;
        targetMask = ~0;
        debugLogs = false;
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

        Vector3 position = startPosition
            + forwardDirection * (speed * aliveTime)
            + rightDirection * (Mathf.Sin(aliveTime * waveFrequency) * waveAmplitude);

        if (ProjectileCollisionUtility.TryApplyDamageAlongSegment(
                previousPosition,
                position,
                hitRadius,
                targetMask,
                owner,
                0f,
                damage,
                out RaycastHit hit))
        {
            Log($"Magic projectile hit: {hit.transform.name}, dmg={damage:0.#}");
            Destroy(gameObject);
            return;
        }

        transform.position = position;
        transform.forward = forwardDirection;
        previousPosition = position;
    }

    private void Log(string message)
    {
        if (!debugLogs)
        {
            return;
        }

        Debug.Log($"[MagicProjectile] {message}", this);
    }
}
