using System;
using UnityEngine;

public enum MagicSpawnSource
{
    AttackOrigin = 0,
    Camera = 1,
    CustomPoint = 2
}

[Serializable]
public struct PhysicalAttackConfig
{
    public float Damage;
    public float Range;
    public float Radius;
    public float Cooldown;
}

[Serializable]
public struct MagicAttackConfig
{
    public float Damage;
    public float Cooldown;
    public float ProjectileSpeed;
    public float ProjectileLifetime;
    public float ProjectileRadius;
    public float ProjectileWaveAmplitude;
    public float ProjectileWaveFrequency;
}

[Serializable]
public struct MagicSpawnConfig
{
    public MagicSpawnSource SpawnSource;
    public Transform CustomSpawnPoint;
    public float SpawnHeightOffset;
    public float SpawnForwardOffset;
}

public readonly struct PlayerCombatConfig
{
    public PlayerCombatConfig(
        PhysicalAttackConfig physicalAttack,
        MagicAttackConfig magicAttack,
        MagicSpawnConfig magicSpawn)
    {
        PhysicalAttack = physicalAttack;
        MagicAttack = magicAttack;
        MagicSpawn = magicSpawn;
    }

    public PhysicalAttackConfig PhysicalAttack { get; }
    public MagicAttackConfig MagicAttack { get; }
    public MagicSpawnConfig MagicSpawn { get; }
}
