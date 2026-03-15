using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class MeleeMob : MonoBehaviour, IDamageSource
{
    [Header("Attack")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 2f;

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
}
