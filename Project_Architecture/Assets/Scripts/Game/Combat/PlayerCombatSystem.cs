using System;
using UnityEngine;

public class PlayerCombatSystem : MonoBehaviour, IPlayerCombatEvents, IMagicCooldownProvider
{
    private enum MagicSpawnSource
    {
        AttackOrigin = 0,
        Camera = 1,
        CustomPoint = 2
    }

    [Header("References")]
    [SerializeField] private MonoBehaviour inputServiceSource;
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

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool logCooldownBlocks;

    public event Action PhysicalAttackPerformed;
    public event Action MagicAttackPerformed;

    public float MagicCooldownDuration => magicCooldown;
    public float MagicCooldownRemaining => combatModel != null ? combatModel.GetMagicCooldownRemaining(Time.time) : 0f;
    public float MagicCooldownNormalized => combatModel != null ? combatModel.GetMagicCooldownNormalized(Time.time) : 0f;
    public bool IsMagicReady => MagicCooldownRemaining <= 0f;

    private IInputService inputService;
    private PlayerCombatModel combatModel;

    private void Awake()
    {
        ResolveInputService();

        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }

        if (attackCamera == null)
        {
            attackCamera = Camera.main;
        }

        combatModel = new PlayerCombatModel(physicalCooldown, magicCooldown);
        Log("Combat system initialized.");
    }

    private void Update()
    {
        if (!ResolveInputService() || combatModel == null)
        {
            return;
        }

        if (inputService.IsPhysicalAttackPressed())
        {
            Log("LMB pressed -> physical attack request.");
            TryPhysicalAttack();
        }

        if (inputService.IsMagicAttackPressed())
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

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            physicalRadius,
            direction,
            physicalRange,
            targetMask,
            QueryTriggerInteraction.Ignore);

        if (hits.Length == 0)
        {
            Log("Physical attack missed.");
            return;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            if (CombatDamageResolver.TryApplyDamage(hit.transform, physicalDamage, 0f))
            {
                Log($"Physical attack hit: {hit.transform.name}, dmg={physicalDamage:0.#}");
                return;
            }
        }

        Log("Physical attack hit collider, but no damage receiver found.");
    }

    private void TryMagicAttack()
    {
        if (magicProjectilePrefab == null)
        {
            Log("Magic projectile prefab is not assigned.");
            return;
        }

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
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

        MagicProjectile projectile = Instantiate(magicProjectilePrefab, spawnPosition, rotation);
        projectile.Initialize(
            transform,
            direction,
            magicDamage,
            magicProjectileSpeed,
            magicProjectileLifetime,
            magicProjectileRadius,
            magicProjectileWaveAmplitude,
            magicProjectileWaveFrequency,
            targetMask,
            enableDebugLogs);

        MagicAttackPerformed?.Invoke();
        Log($"Magic projectile spawned: dmg={magicDamage:0.#}, speed={magicProjectileSpeed:0.#}");
    }

    private Vector3 GetMagicSpawnPosition(Vector3 direction)
    {
        switch (magicSpawnSource)
        {
            case MagicSpawnSource.Camera:
                if (attackCamera != null)
                {
                    return attackCamera.transform.position + direction * magicSpawnForwardOffset;
                }

                break;
            case MagicSpawnSource.CustomPoint:
                if (customMagicSpawnPoint != null)
                {
                    return customMagicSpawnPoint.position;
                }

                break;
        }

        return attackOrigin.position + Vector3.up * magicSpawnHeightOffset;
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

    private bool ResolveInputService()
    {
        return InputServiceResolver.TryResolve(ref inputService, ref inputServiceSource);
    }
}
