using System;
using UnityEngine;

[Serializable]
public struct EnemyVisionConfig
{
    public float ViewDistance;
    [Range(1f, 360f)] public float ViewAngle;
    public float EyeHeight;
    public float VisibilityMemoryDuration;
    public LayerMask VisibilityBlockers;

    public static EnemyVisionConfig CreateDefault()
    {
        return new EnemyVisionConfig
        {
            ViewDistance = 12f,
            ViewAngle = 120f,
            EyeHeight = 1.4f,
            VisibilityMemoryDuration = 1.5f,
            VisibilityBlockers = ~0
        };
    }
}

[Serializable]
public struct EnemyMovementConfig
{
    public bool PatrolLookAroundWhenIdle;
    public float MoveSpeed;
    public float RotationSpeed;
    public float IdleTurnSpeed;
    public bool ConfigureRigidbodyForNavMesh;
    public float NavAcceleration;
    public float NavAngularSpeed;
    public float NavStoppingDistance;
    public float NavPathRepathInterval;

    public static EnemyMovementConfig CreateDefault()
    {
        return new EnemyMovementConfig
        {
            PatrolLookAroundWhenIdle = true,
            MoveSpeed = 3.5f,
            RotationSpeed = 8f,
            IdleTurnSpeed = 45f,
            ConfigureRigidbodyForNavMesh = true,
            NavAcceleration = 16f,
            NavAngularSpeed = 540f,
            NavStoppingDistance = 0.8f,
            NavPathRepathInterval = 0.2f
        };
    }
}

[Serializable]
public struct EnemyAttackConfig
{
    public EnemyAttackKind AttackMode;
    public int Damage;
    public float MeleeAttackRange;
    public float RangedAttackRange;
    public float AttackCooldown;

    public static EnemyAttackConfig CreateDefault()
    {
        return new EnemyAttackConfig
        {
            AttackMode = EnemyAttackKind.Melee,
            Damage = 10,
            MeleeAttackRange = 1.8f,
            RangedAttackRange = 10f,
            AttackCooldown = 1.2f
        };
    }
}

[Serializable]
public struct EnemyProjectileConfig
{
    public GameObject ProjectilePrefab;
    public Transform ProjectileSpawnPoint;
    public float ProjectileSpeed;
    public float ProjectileLifetime;
    public float ProjectileRadius;
    public float ProjectileSpawnHeightOffset;
    public float ProjectileSpawnForwardOffset;
    public LayerMask ProjectileHitMask;

    public static EnemyProjectileConfig CreateDefault()
    {
        return new EnemyProjectileConfig
        {
            ProjectilePrefab = null,
            ProjectileSpawnPoint = null,
            ProjectileSpeed = 14f,
            ProjectileLifetime = 3f,
            ProjectileRadius = 0.2f,
            ProjectileSpawnHeightOffset = 1.2f,
            ProjectileSpawnForwardOffset = 0.4f,
            ProjectileHitMask = ~0
        };
    }
}

public enum EnemyBehaviourMode
{
    Hostile = 0,
    Peaceful = 1
}

[Serializable]
public struct EnemyBehaviourConfig
{
    public EnemyBehaviourMode BehaviourMode;
    [Range(0f, 1f)] public float FleeHealthThreshold;
    [Range(0f, 1f)] public float FleeExitHealthThreshold;
    public float FleeDistance;

    public static EnemyBehaviourConfig CreateDefault()
    {
        return new EnemyBehaviourConfig
        {
            BehaviourMode = EnemyBehaviourMode.Hostile,
            FleeHealthThreshold = 0.25f,
            FleeExitHealthThreshold = 0.45f,
            FleeDistance = 8f
        };
    }
}

[Serializable]
public struct BossCombatConfig
{
    public float StrongAttackCooldown;
    public float StrongAttackDamageMultiplier;
    public bool EnrageBelowHalfHealth;
    public float EnragedAttackSpeedMultiplier;

    public static BossCombatConfig CreateDefault()
    {
        return new BossCombatConfig
        {
            StrongAttackCooldown = 5f,
            StrongAttackDamageMultiplier = 2f,
            EnrageBelowHalfHealth = true,
            EnragedAttackSpeedMultiplier = 1.5f
        };
    }
}
