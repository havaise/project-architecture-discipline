using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IHealth, IDamageable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private bool resetToMaxOnAwake = true;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath;

    private readonly HealthModel model = new HealthModel();

    public int Current => model.Current;
    public int Max => model.Max;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        model.HealthChanged += OnHealthChanged;
        model.Died += OnDied;
        model.Initialize(maxHealth, currentHealth, resetToMaxOnAwake);
    }

    private void OnDestroy()
    {
        model.HealthChanged -= OnHealthChanged;
        model.Died -= OnDied;
    }

    public void TakeDamage(float physicalDamage, float magicDamage)
    {
        int totalDamage = Mathf.RoundToInt(Mathf.Max(0f, physicalDamage) + Mathf.Max(0f, magicDamage));
        model.Reduce(totalDamage);
    }

    public void Reduce(int count)
    {
        model.Reduce(count);
    }

    public void Heal(int amount)
    {
        model.Heal(amount);
    }

    public void SetCurrent(int value)
    {
        model.SetCurrent(value);
    }

    public void SetMaxAndCurrent(int maxValue, int currentValue)
    {
        maxHealth = Mathf.Max(1, maxValue);
        currentHealth = Mathf.Clamp(currentValue, 0, maxHealth);
        model.Initialize(maxHealth, currentHealth, false);
    }

    public bool IsDead()
    {
        return model.IsDead();
    }

    private void OnHealthChanged(int current, int max)
    {
        currentHealth = current;
        maxHealth = max;
        HealthChanged?.Invoke(current, max);
    }

    private void OnDied()
    {
        Died?.Invoke();

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
}
