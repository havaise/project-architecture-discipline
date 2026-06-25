using UnityEngine;

public sealed class MobWeaponRuntime
{
    public MobWeaponRuntime(MobWeaponConfig config)
    {
        Config = config;
    }

    public MobWeaponConfig Config { get; }
    public string WeaponName => Config.WeaponName;
    public EnemyAttackKind AttackKind => Config.AttackKind;
    public int Damage => Config.Damage;
    public float AttackRange => Config.AttackRange;
    public float DamageDelay => Config.DamageDelay;
    public float AttackCooldown => Config.AttackCooldown;
    public GameObject ProjectilePrefab => Config.ProjectilePrefab;
    public float ProjectileSpeed => Config.ProjectileSpeed;
    public float ProjectileLifetime => Config.ProjectileLifetime;
    public float ProjectileRadius => Config.ProjectileRadius;
    public GameObject AttackVfxPrefab => Config.AttackVfxPrefab;
    public GameObject HitVfxPrefab => Config.HitVfxPrefab;
    public AudioClip AttackSfx => Config.AttackSfx;
    public Color ProjectileTint => Config.ProjectileTint;
}
