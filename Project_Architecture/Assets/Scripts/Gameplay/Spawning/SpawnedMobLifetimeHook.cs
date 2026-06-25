using System;
using UnityEngine;

public class SpawnedMobLifetimeHook : MonoBehaviour
{
    private Action<GameObject> onDestroyed;
    private HealthComponent healthComponent;

    public void Initialize(Action<GameObject> callback)
    {
        onDestroyed = callback;
        healthComponent = GetComponentInChildren<HealthComponent>();
        if (healthComponent != null)
        {
            healthComponent.Died += OnDied;
        }
    }

    private void OnDied()
    {
        onDestroyed?.Invoke(gameObject);
    }

    private void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.Died -= OnDied;
        }

        onDestroyed?.Invoke(gameObject);
    }
}
