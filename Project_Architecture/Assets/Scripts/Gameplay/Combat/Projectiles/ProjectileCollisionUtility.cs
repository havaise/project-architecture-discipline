using UnityEngine;

public static class ProjectileCollisionUtility
{
    public static bool TryApplyDamageAlongSegment(
        Vector3 start,
        Vector3 end,
        float hitRadius,
        LayerMask targetMask,
        Transform owner,
        float physicalDamage,
        float magicDamage,
        out RaycastHit hit)
    {
        hit = default;

        Vector3 delta = end - start;
        float distance = delta.magnitude;
        if (distance <= 0.0001f)
        {
            return false;
        }

        Vector3 direction = delta / distance;
        if (!Physics.SphereCast(start, hitRadius, direction, out hit, distance, targetMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        if (IsOwner(owner, hit.transform))
        {
            return false;
        }

        return CombatDamageResolver.TryApplyDamage(hit.transform, physicalDamage, magicDamage);
    }

    public static bool IsOwner(Transform owner, Transform hitTransform)
    {
        if (owner == null || hitTransform == null)
        {
            return false;
        }

        return hitTransform == owner || hitTransform.IsChildOf(owner) || owner.IsChildOf(hitTransform);
    }
}
