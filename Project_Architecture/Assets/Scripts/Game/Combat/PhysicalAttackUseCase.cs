using System;
using UnityEngine;

public sealed class PhysicalAttackUseCase
{
    public bool Execute(
        Transform owner,
        Vector3 origin,
        Vector3 direction,
        float damage,
        float range,
        float radius,
        LayerMask targetMask,
        Action<string> log)
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            radius,
            direction,
            range,
            targetMask,
            QueryTriggerInteraction.Ignore);

        if (hits.Length == 0)
        {
            log?.Invoke("Physical attack missed.");
            return false;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (owner != null && (hit.transform == owner || hit.transform.IsChildOf(owner)))
            {
                continue;
            }

            if (CombatDamageResolver.TryApplyDamage(hit.transform, damage, 0f))
            {
                log?.Invoke($"Physical attack hit: {hit.transform.name}, dmg={damage:0.#}");
                return true;
            }
        }

        log?.Invoke("Physical attack hit collider, but no damage receiver found.");
        return false;
    }
}
