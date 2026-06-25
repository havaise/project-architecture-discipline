using System;

public sealed class HudModel
{
    private readonly IHealth health;
    private readonly IMagicCooldownProvider cooldownProvider;

    public event Action<int, int> HealthChanged;

    public HudModel(IHealth health, IMagicCooldownProvider cooldownProvider)
    {
        this.health = health;
        this.cooldownProvider = cooldownProvider;
    }

    public bool HasHealth => health != null;
    public bool HasCooldown => cooldownProvider != null;
    public int CurrentHealth => health != null ? health.Current : 0;
    public int MaxHealth => health != null ? health.Max : 1;
    public float CooldownRemaining => cooldownProvider != null ? cooldownProvider.MagicCooldownRemaining : 0f;

    public void Subscribe()
    {
        if (health != null)
        {
            health.HealthChanged += OnHealthChanged;
        }
    }

    public void Unsubscribe()
    {
        if (health != null)
        {
            health.HealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        HealthChanged?.Invoke(current, max);
    }
}
