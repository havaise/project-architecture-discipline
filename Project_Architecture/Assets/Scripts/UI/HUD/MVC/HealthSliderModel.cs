using System;

public sealed class HealthSliderModel
{
    private readonly IHealth health;

    public event Action<int, int> HealthChanged;

    public HealthSliderModel(IHealth health)
    {
        this.health = health;
    }

    public int Current => health.Current;
    public int Max => health.Max;

    public void Subscribe()
    {
        health.HealthChanged += OnHealthChanged;
    }

    public void Unsubscribe()
    {
        health.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int current, int max)
    {
        HealthChanged?.Invoke(current, max);
    }
}
