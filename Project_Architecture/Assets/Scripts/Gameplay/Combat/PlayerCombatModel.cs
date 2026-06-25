using UnityEngine;

public sealed class PlayerCombatModel
{
    private readonly float physicalCooldown;
    private readonly float magicCooldown;

    private float nextPhysicalAttackTime;
    private float nextMagicAttackTime;

    public PlayerCombatModel(float physicalCooldown, float magicCooldown)
    {
        this.physicalCooldown = physicalCooldown;
        this.magicCooldown = magicCooldown;
    }

    public bool TryStartPhysicalAttack(float currentTime, out float cooldownRemaining)
    {
        if (currentTime < nextPhysicalAttackTime)
        {
            cooldownRemaining = nextPhysicalAttackTime - currentTime;
            return false;
        }

        nextPhysicalAttackTime = currentTime + physicalCooldown;
        cooldownRemaining = 0f;
        return true;
    }

    public bool TryStartMagicAttack(float currentTime, out float cooldownRemaining)
    {
        if (currentTime < nextMagicAttackTime)
        {
            cooldownRemaining = nextMagicAttackTime - currentTime;
            return false;
        }

        nextMagicAttackTime = currentTime + magicCooldown;
        cooldownRemaining = 0f;
        return true;
    }

    public float GetMagicCooldownRemaining(float currentTime)
    {
        if (nextMagicAttackTime <= currentTime)
        {
            return 0f;
        }

        return nextMagicAttackTime - currentTime;
    }

    public float GetMagicCooldownNormalized(float currentTime)
    {
        if (magicCooldown <= 0.0001f)
        {
            return 0f;
        }

        return Mathf.Clamp01(GetMagicCooldownRemaining(currentTime) / magicCooldown);
    }
}
