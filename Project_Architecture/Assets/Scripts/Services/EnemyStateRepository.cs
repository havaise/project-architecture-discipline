using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyStateRepository : IEnemyStateRepository
{
    private readonly EnemyController[] initialEnemies;

    public EnemyStateRepository(EnemyController[] enemies)
    {
        initialEnemies = enemies;
    }

    public List<EnemySaveData> Capture()
    {
        List<EnemySaveData> data = new List<EnemySaveData>();
        EnemyController[] enemies = GetEnemiesSnapshot();
        if (enemies == null)
        {
            return data;
        }

        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyController enemy = enemies[i];
            if (enemy == null)
            {
                continue;
            }

            HealthComponent health = enemy.GetComponentInChildren<HealthComponent>();
            EnemySaveId idComponent = enemy.GetComponent<EnemySaveId>();

            data.Add(new EnemySaveData
            {
                Id = idComponent != null ? idComponent.Id : enemy.name,
                Position = enemy.transform.position,
                Rotation = enemy.transform.rotation,
                CurrentHp = health != null ? health.Current : 0,
                MaxHp = health != null ? health.Max : 0
            });
        }

        return data;
    }

    public void Restore(List<EnemySaveData> enemiesData)
    {
        EnemyController[] enemies = GetEnemiesSnapshot();
        if (enemies == null || enemiesData == null || enemiesData.Count == 0)
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

        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyController enemy = enemies[i];
            if (enemy == null)
            {
                continue;
            }

            string key = ResolveEnemyKey(enemy);
            if (string.IsNullOrWhiteSpace(key) || !byId.TryGetValue(key, out List<EnemySaveData> candidates))
            {
                continue;
            }

            EnemySaveData data = TakeBestCandidate(candidates, enemy.transform.position);
            enemy.transform.SetPositionAndRotation(data.Position, data.Rotation);

            HealthComponent health = enemy.GetComponentInChildren<HealthComponent>();
            if (health != null)
            {
                health.SetCurrent(data.CurrentHp);
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

    private static string ResolveEnemyKey(EnemyController enemy)
    {
        EnemySaveId idComponent = enemy.GetComponent<EnemySaveId>();
        if (idComponent != null && !string.IsNullOrWhiteSpace(idComponent.Id))
        {
            return idComponent.Id;
        }

        return enemy.name;
    }

    private EnemyController[] GetEnemiesSnapshot()
    {
        EnemyController[] sceneEnemies = UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        if (sceneEnemies != null && sceneEnemies.Length > 0)
        {
            return sceneEnemies;
        }

        return initialEnemies;
    }
}
