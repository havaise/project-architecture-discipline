using UnityEngine;

public enum BossElement
{
    Ice = 0,
    Fire = 1,
    Earth = 2,
    Ether = 3
}

public enum BossAttackType
{
    Melee = 0,
    Ranged = 1
}

[CreateAssetMenu(menuName = "Game/AI/Mob Weapon Config", fileName = "MobWeaponConfig")]
public class MobWeaponConfig : ScriptableObject
{
    [Header("Identity")]
    public string WeaponName = "Weapon";
    public EnemyAttackKind AttackKind = EnemyAttackKind.Melee;

    [Header("Combat")]
    public int Damage = 10;
    public float AttackRange = 2f;
    public float DamageDelay = 0f;
    public float AttackCooldown = 1.2f;

    [Header("Projectile (For Ranged)")]
    public GameObject ProjectilePrefab;
    public float ProjectileSpeed = 14f;
    public float ProjectileLifetime = 3f;
    public float ProjectileRadius = 0.2f;

    [Header("Presentation")]
    public GameObject AttackVfxPrefab;
    public GameObject HitVfxPrefab;
    public AudioClip AttackSfx;
    public Color ProjectileTint = Color.white;
}

[System.Serializable]
public struct BossElementAttackProfile
{
    public BossAttackType AttackType;
    public BossElement Element;
    public float DamageMultiplier;
    public float CooldownMultiplier;
    public float ProjectileSpeedMultiplier;
    public Color Color;
    public GameObject AttackVfxPrefab;
    public GameObject HitVfxPrefab;
    public AudioClip Sound;
    [TextArea] public string Description;
}
