using System;
using UnityEngine;

public sealed class MagicAttackUseCase
{
    private readonly IPlayerProjectileFactory projectileFactory;

    public MagicAttackUseCase(IPlayerProjectileFactory projectileFactory)
    {
        this.projectileFactory = projectileFactory ?? throw new ArgumentNullException(nameof(projectileFactory));
    }

    public bool Execute(
        MagicProjectile projectilePrefab,
        Transform owner,
        Vector3 spawnPosition,
        Vector3 direction,
        MagicAttackConfig config,
        LayerMask targetMask,
        bool enableDebugLogs,
        GameObject hitImpactVfxPrefab,
        float autoAimRadius,
        float autoAimTurnSpeed,
        Action<string> log)
    {
        if (projectilePrefab == null)
        {
            log?.Invoke("Magic projectile prefab is not assigned.");
            return false;
        }

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        MagicProjectile projectile = projectileFactory.Create(projectilePrefab, spawnPosition, rotation);
        if (projectile == null)
        {
            log?.Invoke("Magic projectile factory returned null.");
            return false;
        }

        projectile.Initialize(
            owner,
            direction,
            config.Damage,
            config.ProjectileSpeed,
            config.ProjectileLifetime,
            config.ProjectileRadius,
            config.ProjectileWaveAmplitude,
            config.ProjectileWaveFrequency,
            targetMask,
            enableDebugLogs,
            hitImpactVfxPrefab,
            autoAimRadius,
            autoAimTurnSpeed);

        log?.Invoke($"Magic projectile spawned: dmg={config.Damage:0.#}, speed={config.ProjectileSpeed:0.#}");
        return true;
    }
}
