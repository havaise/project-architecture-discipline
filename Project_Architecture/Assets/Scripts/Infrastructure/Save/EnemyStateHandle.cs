using UnityEngine;

public sealed class EnemyStateHandle
{
    public EnemyStateHandle(Component actor)
    {
        Actor = actor;
        if (actor == null)
        {
            return;
        }

        Transform actorTransform = actor.transform;
        Health = actorTransform.GetComponentInChildren<HealthComponent>();
        EnemySaveId idComponent = actorTransform.GetComponent<EnemySaveId>();
        StableId = idComponent != null && !string.IsNullOrWhiteSpace(idComponent.Id)
            ? idComponent.Id
            : actorTransform.name;
    }

    public Component Actor { get; }
    public Transform Transform => Actor != null ? Actor.transform : null;
    public HealthComponent Health { get; }
    public string StableId { get; }
}
