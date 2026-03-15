using UnityEngine;

public class RangedMob : MonoBehaviour, IDamage, IDamageSource, IHealth
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 80;
    [SerializeField] private int currentHealth = 80;

    [Header("Attack")]
    [SerializeField] private int damage = 7;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 1.5f;

    private float nextAttackTime;

    public int _current => currentHealth;
    public int _max => maxHealth;

    private void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

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

    public void TakeDamage(int amount)
    {
        Reduce(amount);
    }

    public void Reduce(int count)
    {
        if (count <= 0 || isDead())
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - count);

        if (isDead())
        {
            OnDeath();
        }
    }

    public bool isDead()
    {
        return currentHealth <= 0;
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }
}
