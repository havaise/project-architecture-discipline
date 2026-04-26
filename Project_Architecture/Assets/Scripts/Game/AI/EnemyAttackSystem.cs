using System;
using UnityEngine;

public enum EnemyAttackKind
{
    Melee = 0,
    Ranged = 1
}

public enum EnemyAttackResult
{
    None = 0,
    Cooldown = 1,
    MeleeHit = 2,
    MeleeMiss = 3,
    RangedFired = 4,
    RangedBlocked = 5
}

public sealed class EnemyAttackSystem
{
    private readonly Transform owner;
    private readonly EnemyAiModel aiModel;
    private readonly Func<string, EnemyProjectile> projectileFactory;

    public EnemyAttackSystem(
        Transform owner,
        EnemyAiModel aiModel,
        Func<string, EnemyProjectile> projectileFactory)
    {
        this.owner = owner != null
            ? owner
            : throw new ArgumentNullException(nameof(owner));
        this.aiModel = aiModel != null
            ? aiModel
            : throw new ArgumentNullException(nameof(aiModel));
        this.projectileFactory = projectileFactory != null
            ? projectileFactory
            : throw new ArgumentNullException(nameof(projectileFactory));
    }

    public bool IsInRange(Transform target, EnemyAttackKind attackKind, float meleeRange, float rangedRange)
    {
        if (target == null)
        {
            return false;
        }

        float range = attackKind == EnemyAttackKind.Ranged ? rangedRange : meleeRange;
        return Vector3.Distance(owner.position, target.position) <= range;
    }

    public EnemyAttackResult TryAttack(
        Transform target,
        EnemyAttackKind attackKind,
        int damage,
        float currentTime,
        float attackSpeedMultiplier,
        float projectileSpeed,
        float projectileLifetime,
        float projectileRadius,
        Transform projectileSpawnPoint,
        float projectileSpawnHeightOffset,
        float projectileSpawnForwardOffset,
        LayerMask projectileHitMask,
        bool enableCombatDebugLogs,
        out float cooldownRemaining)
    {
        cooldownRemaining = 0f;

        if (target == null)
        {
            return EnemyAttackResult.None;
        }

        if (!aiModel.TryConsumeAttack(currentTime, out cooldownRemaining, attackSpeedMultiplier))
        {
            return EnemyAttackResult.Cooldown;
        }

        if (attackKind == EnemyAttackKind.Ranged)
        {
            return TryRangedAttack(
                target,
                Mathf.Max(0, damage),
                projectileSpeed,
                projectileLifetime,
                projectileRadius,
                projectileSpawnPoint,
                projectileSpawnHeightOffset,
                projectileSpawnForwardOffset,
                projectileHitMask,
                enableCombatDebugLogs)
                ? EnemyAttackResult.RangedFired
                : EnemyAttackResult.RangedBlocked;
        }

        return CombatDamageResolver.TryApplyDamage(target, Mathf.Max(0, damage), 0f)
            ? EnemyAttackResult.MeleeHit
            : EnemyAttackResult.MeleeMiss;
    }

    private bool TryRangedAttack(
        Transform target,
        int damage,
        float projectileSpeed,
        float projectileLifetime,
        float projectileRadius,
        Transform projectileSpawnPoint,
        float projectileSpawnHeightOffset,
        float projectileSpawnForwardOffset,
        LayerMask projectileHitMask,
        bool enableCombatDebugLogs)
    {
        Vector3 spawnPosition = GetProjectileSpawnPosition(
            projectileSpawnPoint,
            projectileSpawnHeightOffset,
            projectileSpawnForwardOffset);
        Vector3 targetPoint = target.position + Vector3.up * projectileSpawnHeightOffset;
        Vector3 direction = targetPoint - spawnPosition;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = owner.forward;
        }

        direction.Normalize();

        EnemyProjectile projectile = projectileFactory.Invoke("EnemyProjectile");
        if (projectile == null)
        {
            return false;
        }

        projectile.transform.SetPositionAndRotation(spawnPosition, Quaternion.LookRotation(direction, Vector3.up));
        projectile.Initialize(
            owner,
            direction,
            damage,
            projectileSpeed,
            projectileLifetime,
            projectileRadius,
            projectileHitMask,
            enableCombatDebugLogs);

        return true;
    }

    private Vector3 GetProjectileSpawnPosition(
        Transform projectileSpawnPoint,
        float projectileSpawnHeightOffset,
        float projectileSpawnForwardOffset)
    {
        if (projectileSpawnPoint != null)
        {
            return projectileSpawnPoint.position;
        }

        return owner.position
            + Vector3.up * projectileSpawnHeightOffset
            + owner.forward * projectileSpawnForwardOffset;
    }
}
