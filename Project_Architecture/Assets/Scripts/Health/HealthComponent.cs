using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IHealth, IDamage, IDamageable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private bool resetToMaxOnAwake = true;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath;

    public int _current => currentHealth;
    public int _max => maxHealth;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = resetToMaxOnAwake ? maxHealth : Mathf.Clamp(currentHealth, 0, maxHealth);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        Reduce(amount);
    }

    public void TakeDamage(float physicalDamage, float magicDamage)
    {
        int totalDamage = Mathf.RoundToInt(Mathf.Max(0f, physicalDamage) + Mathf.Max(0f, magicDamage));
        Reduce(totalDamage);
    }

    public void Reduce(int count)
    {
        if (count <= 0 || isDead())
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - count);
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (isDead())
        {
            Died?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || isDead())
        {
            return;
        }

        int newHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if (newHealth == currentHealth)
        {
            return;
        }

        currentHealth = newHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public bool isDead()
    {
        return currentHealth <= 0;
    }
}
