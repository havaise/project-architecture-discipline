using System;
using UnityEngine;

public sealed class EnemyVisionSensor
{
    private readonly Transform observer;

    public EnemyVisionSensor(Transform observer)
    {
        this.observer = observer != null
            ? observer
            : throw new ArgumentNullException(nameof(observer));
    }

    public bool CanSee(
        Transform target,
        float viewDistance,
        float viewAngle,
        float eyeHeight,
        LayerMask visibilityBlockers)
    {
        if (target == null)
        {
            return false;
        }

        Vector3 origin = observer.position + Vector3.up * eyeHeight;
        Vector3 targetPosition = target.position + Vector3.up * eyeHeight;
        Vector3 toTarget = targetPosition - origin;

        if (toTarget.sqrMagnitude > viewDistance * viewDistance)
        {
            return false;
        }

        float angle = Vector3.Angle(observer.forward, toTarget);
        if (angle > viewAngle * 0.5f)
        {
            return false;
        }

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            toTarget.normalized,
            viewDistance,
            visibilityBlockers,
            QueryTriggerInteraction.Ignore);

        if (hits.Length == 0)
        {
            return true;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.transform;

            if (hitTransform == observer || hitTransform.IsChildOf(observer))
            {
                continue;
            }

            return hitTransform == target || hitTransform.IsChildOf(target);
        }

        return true;
    }
}
