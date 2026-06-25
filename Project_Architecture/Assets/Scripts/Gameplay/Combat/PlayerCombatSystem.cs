using System;
using UnityEngine;

public class PlayerCombatSystem : MonoBehaviour, IPlayerCombatEvents, IMagicCooldownProvider
{
    [Header("References")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private Camera attackCamera;

    [Header("Physical Attack (LMB)")]
    [SerializeField] private float physicalDamage = 20f;
    [SerializeField] private float physicalRange = 2.2f;
    [SerializeField] private float physicalRadius = 0.8f;
    [SerializeField] private float physicalCooldown = 0.35f;

    [Header("Magic Attack (RMB)")]
    [SerializeField] private float magicDamage = 30f;
    [SerializeField] private float magicCooldown = 0.8f;
    [SerializeField] private MagicProjectile magicProjectilePrefab;
    [SerializeField] private float magicProjectileSpeed = 12f;
    [SerializeField] private float magicProjectileLifetime = 2.5f;
    [SerializeField] private float magicProjectileRadius = 0.25f;
    [SerializeField] private float magicProjectileWaveAmplitude = 0.6f;
    [SerializeField] private float magicProjectileWaveFrequency = 8f;

    [Header("Magic Spawn")]
    [SerializeField] private MagicSpawnSource magicSpawnSource = MagicSpawnSource.AttackOrigin;
    [SerializeField] private Transform customMagicSpawnPoint;
    [SerializeField] private float magicSpawnHeightOffset = 1.0f;
    [SerializeField] private float magicSpawnForwardOffset = 0.5f;

    [Header("Targeting")]
    [SerializeField] private LayerMask targetMask = ~0;

    [Header("Presentation")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject physicalAttackVfxPrefab;
    [SerializeField] private AudioClip physicalAttackSfx;
    [SerializeField] private GameObject magicShotVfxPrefab;
    [SerializeField] private AudioClip magicShotSfx;
    [SerializeField] private GameObject magicHitImpactVfxPrefab;

    [Header("Magic Auto-Aim")]
    [SerializeField] private float magicAutoAimRadius = 1.75f;
    [SerializeField] private float magicAutoAimTurnSpeed = 360f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool logCooldownBlocks;

    public event Action PhysicalAttackPerformed;
    public event Action MagicAttackPerformed;

    public float MagicCooldownDuration => magicCooldown;
    public float MagicCooldownRemaining => combatModel != null ? combatModel.GetMagicCooldownRemaining(Time.time) : 0f;
    public float MagicCooldownNormalized => combatModel != null ? combatModel.GetMagicCooldownNormalized(Time.time) : 0f;
    public bool IsMagicReady => MagicCooldownRemaining <= 0f;

    private PlayerCombatModel combatModel;
    private PlayerCombatConfig combatConfig;
    private PhysicalAttackUseCase physicalAttackUseCase;
    private MagicAttackUseCase magicAttackUseCase;

    private void Awake()
    {
        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }

        if (attackCamera == null)
        {
            attackCamera = Camera.main;
        }

        combatConfig = new PlayerCombatConfig(
            new PhysicalAttackConfig
            {
                Damage = physicalDamage,
                Range = physicalRange,
                Radius = physicalRadius,
                Cooldown = physicalCooldown
            },
            new MagicAttackConfig
            {
                Damage = magicDamage,
                Cooldown = magicCooldown,
                ProjectileSpeed = magicProjectileSpeed,
                ProjectileLifetime = magicProjectileLifetime,
                ProjectileRadius = magicProjectileRadius,
                ProjectileWaveAmplitude = magicProjectileWaveAmplitude,
                ProjectileWaveFrequency = magicProjectileWaveFrequency
            },
            new MagicSpawnConfig
            {
                SpawnSource = magicSpawnSource,
                CustomSpawnPoint = customMagicSpawnPoint,
                SpawnHeightOffset = magicSpawnHeightOffset,
                SpawnForwardOffset = magicSpawnForwardOffset
            });

        combatModel = new PlayerCombatModel(physicalCooldown, magicCooldown);
        physicalAttackUseCase = new PhysicalAttackUseCase();
        magicAttackUseCase = new MagicAttackUseCase(new UnityPlayerProjectileFactory());
        Log("Combat system initialized.");
    }

    public void ProcessFrame(PlayerModel input)
    {
        if (combatModel == null || input == null)
        {
            return;
        }

        if (input.PhysicalAttackPressed)
        {
            Log("LMB pressed -> physical attack request.");
            TryPhysicalAttack();
        }

        if (input.MagicAttackPressed)
        {
            Log("RMB pressed -> magic attack request.");
            TryMagicAttack();
        }
    }

    private void TryPhysicalAttack()
    {
        if (!combatModel.TryStartPhysicalAttack(Time.time, out float cooldownRemaining))
        {
            if (logCooldownBlocks)
            {
                Log($"Physical blocked by cooldown: {cooldownRemaining:0.00}s left.");
            }

            return;
        }

        PhysicalAttackPerformed?.Invoke();

        Vector3 origin = attackOrigin.position + Vector3.up * 1.0f;
        Vector3 direction = GetForwardDirection();
        physicalAttackUseCase.Execute(
            transform,
            origin,
            direction,
            combatConfig.PhysicalAttack.Damage,
            combatConfig.PhysicalAttack.Range,
            combatConfig.PhysicalAttack.Radius,
            targetMask,
            Log);
        PlayAttackPresentation(physicalAttackVfxPrefab, physicalAttackSfx, origin);
    }

    private void TryMagicAttack()
    {
        if (!combatModel.TryStartMagicAttack(Time.time, out float cooldownRemaining))
        {
            if (logCooldownBlocks)
            {
                Log($"Magic blocked by cooldown: {cooldownRemaining:0.00}s left.");
            }

            return;
        }

        Vector3 direction = GetForwardDirection();
        Vector3 spawnPosition = GetMagicSpawnPosition(direction);
        bool spawned = magicAttackUseCase.Execute(
            magicProjectilePrefab,
            transform,
            spawnPosition,
            direction,
            combatConfig.MagicAttack,
            targetMask,
            enableDebugLogs,
            magicHitImpactVfxPrefab,
            magicAutoAimRadius,
            magicAutoAimTurnSpeed,
            Log);
        if (!spawned)
        {
            return;
        }

        MagicAttackPerformed?.Invoke();
        PlayAttackPresentation(magicShotVfxPrefab, magicShotSfx, spawnPosition);
    }

    private Vector3 GetMagicSpawnPosition(Vector3 direction)
    {
        switch (combatConfig.MagicSpawn.SpawnSource)
        {
            case MagicSpawnSource.Camera:
                if (attackCamera != null)
                {
                    return attackCamera.transform.position + direction * combatConfig.MagicSpawn.SpawnForwardOffset;
                }

                break;
            case MagicSpawnSource.CustomPoint:
                if (combatConfig.MagicSpawn.CustomSpawnPoint != null)
                {
                    return combatConfig.MagicSpawn.CustomSpawnPoint.position;
                }

                break;
        }

        return attackOrigin.position + Vector3.up * combatConfig.MagicSpawn.SpawnHeightOffset;
    }

    private Vector3 GetForwardDirection()
    {
        if (attackCamera != null)
        {
            Vector3 forward = attackCamera.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.0001f)
            {
                return forward.normalized;
            }
        }

        return transform.forward;
    }

    private void Log(string message)
    {
        if (!enableDebugLogs)
        {
            return;
        }

        Debug.Log($"[PlayerCombatSystem] {message}", this);
    }

    private void PlayAttackPresentation(GameObject vfxPrefab, AudioClip sfx, Vector3 position)
    {
        if (vfxPrefab != null)
        {
            Instantiate(vfxPrefab, position, Quaternion.identity);
        }

        if (sfx == null)
        {
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(sfx);
            return;
        }

        AudioSource.PlayClipAtPoint(sfx, position);
    }
}
