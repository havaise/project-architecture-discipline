using UnityEngine;

public static class CombatDamageResolver
{
    public static bool TryApplyDamage(Component target, float physicalDamage, float magicDamage)
    {
        if (target == null)
        {
            return false;
        }

        IDamageable damageable = FindDamageable(target.transform);
        if (damageable == null)
        {
            return false;
        }

        damageable.TakeDamage(physicalDamage, magicDamage);
        return true;
    }

    private static IDamageable FindDamageable(Transform targetTransform)
    {
        IDamageable damageable = targetTransform.GetComponent<IDamageable>();
        if (damageable != null)
        {
            return damageable;
        }

        damageable = targetTransform.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            return damageable;
        }

        return targetTransform.GetComponentInChildren<IDamageable>();
    }
}
