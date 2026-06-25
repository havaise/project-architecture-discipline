using System;
using UnityEngine;

public sealed class HealthModel
{
    private int maxHealth;
    private int currentHealth;

    public int Current => currentHealth;
    public int Max => maxHealth;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    public void Initialize(int initialMaxHealth, int initialCurrentHealth, bool resetToMax)
    {
        maxHealth = Mathf.Max(1, initialMaxHealth);
        currentHealth = resetToMax ? maxHealth : Mathf.Clamp(initialCurrentHealth, 0, maxHealth);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Reduce(int count)
    {
        if (count <= 0 || IsDead())
        {
            return;
        }

        int newHealth = Mathf.Max(0, currentHealth - count);
        ApplyCurrentHealth(newHealth);
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead())
        {
            return;
        }

        int newHealth = Mathf.Min(maxHealth, currentHealth + amount);
        ApplyCurrentHealth(newHealth);
    }

    public void SetCurrent(int value)
    {
        int newHealth = Mathf.Clamp(value, 0, maxHealth);
        ApplyCurrentHealth(newHealth);
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    private void ApplyCurrentHealth(int newHealth)
    {
        bool wasAlive = currentHealth > 0;
        if (currentHealth == newHealth)
        {
            return;
        }

        currentHealth = newHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (wasAlive && currentHealth <= 0)
        {
            Died?.Invoke();
        }
    }
}
