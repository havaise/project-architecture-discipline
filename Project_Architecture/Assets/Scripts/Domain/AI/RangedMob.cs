using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class RangedMob : MonoBehaviour, IDamageSource
{
    [Header("Attack")]
    [SerializeField] private int damage = 7;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 14f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private float projectileRadius = 0.2f;
    [SerializeField] private float projectileSpawnHeightOffset = 1.2f;
    [SerializeField] private float projectileSpawnForwardOffset = 0.4f;
    [SerializeField] private LayerMask projectileHitMask = ~0;
    [SerializeField] private bool enableDebugLogs;

    private float nextAttackTime;

    public int GetDamage()
    {
        return damage;
    }

    public bool CanAttack(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        return Vector3.Distance(transform.position, target.position) <= attackRange;
    }

    public bool IsAttackReady()
    {
        return Time.time >= nextAttackTime;
    }

    public bool TryAttack(Transform target)
    {
        if (!CanAttack(target) || !IsAttackReady())
        {
            return false;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        Vector3 targetPoint = target.position + Vector3.up * projectileSpawnHeightOffset;
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
            damage,
            projectileSpeed,
            projectileLifetime,
            projectileRadius,
            projectileHitMask,
            enableDebugLogs);

        nextAttackTime = Time.time + attackCooldown;
        Log($"Projectile fired: dmg={damage}, speed={projectileSpeed:0.##}");
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

    private Vector3 GetSpawnPosition()
    {
        if (projectileSpawnPoint != null)
        {
            return projectileSpawnPoint.position;
        }

        return transform.position
            + Vector3.up * projectileSpawnHeightOffset
            + transform.forward * projectileSpawnForwardOffset;
    }

    private void Log(string message)
    {
        if (!enableDebugLogs)
        {
            return;
        }

        Debug.Log($"[RangedMob] {message}", this);
    }
}
