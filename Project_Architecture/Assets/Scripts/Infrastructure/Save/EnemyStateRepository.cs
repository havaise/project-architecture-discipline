using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyStateRepository : IEnemyStateRepository
{
    private readonly IReadOnlyList<EnemyStateHandle> enemyHandles;

    public EnemyStateRepository(IReadOnlyList<EnemyStateHandle> enemyHandles)
    {
        this.enemyHandles = enemyHandles ?? Array.Empty<EnemyStateHandle>();
    }

    public List<EnemySaveData> Capture()
    {
        List<EnemySaveData> data = new List<EnemySaveData>();
        for (int i = 0; i < enemyHandles.Count; i++)
        {
            EnemyStateHandle handle = enemyHandles[i];
            Transform actorTransform = handle != null ? handle.Transform : null;
            if (actorTransform == null || string.IsNullOrWhiteSpace(handle.StableId))
            {
                continue;
            }

            data.Add(new EnemySaveData
            {
                Id = handle.StableId,
                Position = actorTransform.position,
                Rotation = actorTransform.rotation,
                CurrentHp = handle.Health != null ? handle.Health.Current : 0,
                MaxHp = handle.Health != null ? handle.Health.Max : 0
            });
        }

        return data;
    }

    public void Restore(List<EnemySaveData> enemiesData)
    {
        if (enemiesData == null || enemiesData.Count == 0)
        {
            return;
        }

        Dictionary<string, List<EnemySaveData>> byId = new Dictionary<string, List<EnemySaveData>>();
        for (int i = 0; i < enemiesData.Count; i++)
        {
            EnemySaveData data = enemiesData[i];
            if (data == null || string.IsNullOrWhiteSpace(data.Id))
            {
                continue;
            }

            if (!byId.TryGetValue(data.Id, out List<EnemySaveData> list))
            {
                list = new List<EnemySaveData>();
                byId[data.Id] = list;
            }

            list.Add(data);
        }

        for (int i = 0; i < enemyHandles.Count; i++)
        {
            EnemyStateHandle handle = enemyHandles[i];
            Transform actorTransform = handle != null ? handle.Transform : null;
            if (actorTransform == null || string.IsNullOrWhiteSpace(handle.StableId))
            {
                continue;
            }

            if (!byId.TryGetValue(handle.StableId, out List<EnemySaveData> candidates))
            {
                continue;
            }

            EnemySaveData data = TakeBestCandidate(candidates, actorTransform.position);
            actorTransform.SetPositionAndRotation(data.Position, data.Rotation);

            if (handle.Health != null)
            {
                handle.Health.SetCurrent(data.CurrentHp);
            }
        }
    }

    private static EnemySaveData TakeBestCandidate(List<EnemySaveData> candidates, Vector3 currentEnemyPosition)
    {
        if (candidates.Count == 1)
        {
            EnemySaveData single = candidates[0];
            candidates.RemoveAt(0);
            return single;
        }

        int bestIndex = 0;
        float bestDistanceSqr = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            float distanceSqr = (candidates[i].Position - currentEnemyPosition).sqrMagnitude;
            if (distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                bestIndex = i;
            }
        }

        EnemySaveData best = candidates[bestIndex];
        candidates.RemoveAt(bestIndex);
        return best;
    }
}
