using UnityEngine;

public class MeleeMob : MonoBehaviour, IDamage, IDamageSource, IHealth
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Attack")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 2f;

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
