using System;
using UnityEngine;

public interface IEnemyProjectileFactory
{
    EnemyProjectile CreateProjectile();
}

public sealed class UnityEnemyProjectileFactory : IEnemyProjectileFactory
{
    private readonly GameObject projectilePrefab;
    private readonly Action<string> log;

    public UnityEnemyProjectileFactory(GameObject projectilePrefab, Action<string> log)
    {
        this.projectilePrefab = projectilePrefab;
        this.log = log;
    }

    public EnemyProjectile CreateProjectile()
    {
        if (projectilePrefab == null)
        {
            log?.Invoke("Projectile prefab is not assigned.");
            return null;
        }

        GameObject projectileObject = UnityEngine.Object.Instantiate(projectilePrefab);
        if (projectileObject.TryGetComponent(out EnemyProjectile projectile))
        {
            return projectile;
        }

        log?.Invoke("Projectile prefab has no EnemyProjectile component.");
        UnityEngine.Object.Destroy(projectileObject);
        return null;
    }
}
