using UnityEngine;

public static class CombatDamageResolver
{
    public static bool TryApplyDamage(Component target, float physicalDamage, float magicDamage)
    {
        if (target == null)
        {
            return false;
        }

        if (TryGetDamageReceivers(target.transform, out IDamage intDamage, out IDamageable mixedDamage))
        {
            if (mixedDamage != null)
            {
                mixedDamage.TakeDamage(physicalDamage, magicDamage);
                return true;
            }

            int totalDamage = Mathf.RoundToInt(Mathf.Max(0f, physicalDamage) + Mathf.Max(0f, magicDamage));
            if (totalDamage <= 0)
            {
                return false;
            }

            intDamage.TakeDamage(totalDamage);
            return true;
        }

        return false;
    }

    private static bool TryGetDamageReceivers(Transform targetTransform, out IDamage damage, out IDamageable damageable)
    {
        damage = targetTransform.GetComponent<IDamage>();
        if (damage == null)
        {
            damage = targetTransform.GetComponentInParent<IDamage>();
        }

        if (damage == null)
        {
            damage = targetTransform.GetComponentInChildren<IDamage>();
        }

        damageable = targetTransform.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = targetTransform.GetComponentInParent<IDamageable>();
        }

        if (damageable == null)
        {
            damageable = targetTransform.GetComponentInChildren<IDamageable>();
        }

        return damage != null || damageable != null;
    }
}
