using UnityEngine;

public sealed class EnemyStateHandle
{
    public EnemyStateHandle(EnemyController enemy)
    {
        Enemy = enemy;
        if (enemy == null)
        {
            return;
        }

        Health = enemy.GetComponentInChildren<HealthComponent>();
        EnemySaveId idComponent = enemy.GetComponent<EnemySaveId>();
        StableId = idComponent != null && !string.IsNullOrWhiteSpace(idComponent.Id)
            ? idComponent.Id
            : enemy.name;
    }

    public EnemyController Enemy { get; }
    public HealthComponent Health { get; }
    public string StableId { get; }
}
