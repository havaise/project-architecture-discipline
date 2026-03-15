using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class RangedMob : MonoBehaviour, IDamageSource
{
    [Header("Attack")]
    [SerializeField] private int damage = 7;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 1.5f;

    private float nextAttackTime;

    public int GetDamage()
    {
        return damage;
    }

    public bool CanAttack(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        return Vector3.Distance(transform.position, target.position) <= attackRange;
    }

    public bool IsAttackReady()
    {
        return Time.time >= nextAttackTime;
    }

    public bool TryAttack(Transform target)
    {
        if (!CanAttack(target) || !IsAttackReady())
        {
            return false;
        }

        nextAttackTime = Time.time + attackCooldown;
        return true;
    }
}
