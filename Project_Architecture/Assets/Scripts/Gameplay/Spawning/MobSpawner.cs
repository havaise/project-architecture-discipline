using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum SpawnMode
{
    ExactPosition = 0,
    RandomPointInRadius = 1,
    RandomFromPointList = 2
}

public class MobSpawner : MonoBehaviour
{
    [Header("Spawn Set")]
    [SerializeField] private GameObject[] mobPrefabs;
    [SerializeField] private GameObject[] rareMobPrefabs;
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private float firstSpawnDelay = 0f;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxAlive = 8;
    [SerializeField] private bool spawnContinuously = true;
    [SerializeField] private bool autoStartOnEnable = true;

    [Header("Spawn Points")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.RandomPointInRadius;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool sampleNavMesh = true;
    [SerializeField] private float navMeshSampleDistance = 2.5f;

    [Header("Mob Weapons")]
    [SerializeField] private MobWeaponConfig[] overrideWeaponPool;
    [SerializeField] private bool randomizeWeaponFromPool = true;

    [Header("Rare Mob")]
    [Range(0f, 1f)] [SerializeField] private float rareMobChance = 0.1f;
    [SerializeField] private float rareHealthMultiplier = 1.75f;
    [SerializeField] private float rareDamageMultiplier = 1.35f;
    [SerializeField] private float rareSpeedMultiplier = 1.1f;
    [SerializeField] private float rareScaleMultiplier = 1.2f;
    [SerializeField] private Material rareMaterial;
    [SerializeField] private GameObject rareVfxPrefab;

    private readonly List<GameObject> aliveMobs = new List<GameObject>();
    private Coroutine spawnRoutine;
    private int spawnedSoFar;

    private void OnEnable()
    {
        if (autoStartOnEnable)
        {
            StartSpawning();
        }
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        CleanupAliveList();
    }

    [ContextMenu("Spawn One Mob Now")]
    public void SpawnOneNow()
    {
        TrySpawnOne();
    }

    [ContextMenu("Start Spawner")]
    public void StartSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        if (firstSpawnDelay > 0f)
        {
            yield return new WaitForSeconds(firstSpawnDelay);
        }

        while (spawnContinuously || spawnedSoFar < Mathf.Max(0, spawnCount))
        {
            TrySpawnOne();
            yield return new WaitForSeconds(Mathf.Max(0.05f, spawnInterval));
        }

        spawnRoutine = null;
    }

    private void TrySpawnOne()
    {
        CleanupAliveList();
        if (aliveMobs.Count >= Mathf.Max(1, maxAlive))
        {
            return;
        }

        GameObject prefab = PickPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("[MobSpawner] No mob prefabs assigned.", this);
            return;
        }

        bool spawnRare = Random.value <= rareMobChance;
        if (spawnRare && rareMobPrefabs != null && rareMobPrefabs.Length > 0)
        {
            GameObject rarePrefab = rareMobPrefabs[Random.Range(0, rareMobPrefabs.Length)];
            if (rarePrefab != null)
            {
                prefab = rarePrefab;
            }
        }

        Vector3 spawnPosition = ResolveSpawnPosition();
        GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity);
        aliveMobs.Add(instance);
        spawnedSoFar++;

        SpawnedMobLifetimeHook hook = instance.GetComponent<SpawnedMobLifetimeHook>();
        if (hook == null)
        {
            hook = instance.AddComponent<SpawnedMobLifetimeHook>();
        }

        hook.Initialize(OnSpawnedMobDestroyed);

        EnemyController controller = instance.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.SetSpawnWeaponOptions(overrideWeaponPool, randomizeWeaponFromPool);
            if (spawnRare)
            {
                controller.ApplySpawnMultipliers(rareHealthMultiplier, rareDamageMultiplier, rareSpeedMultiplier);
            }
        }

        if (spawnRare)
        {
            ApplyRareVisual(instance);
        }
    }

    private void ApplyRareVisual(GameObject mob)
    {
        mob.transform.localScale *= Mathf.Max(0.1f, rareScaleMultiplier);
        if (rareMaterial != null)
        {
            Renderer[] renderers = mob.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = rareMaterial;
            }
        }

        if (rareVfxPrefab != null)
        {
            Instantiate(rareVfxPrefab, mob.transform.position, Quaternion.identity, mob.transform);
        }
    }

    private Vector3 ResolveSpawnPosition()
    {
        switch (spawnMode)
        {
            case SpawnMode.RandomFromPointList:
                if (spawnPoints != null && spawnPoints.Length > 0)
                {
                    Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                    if (point != null)
                    {
                        return point.position;
                    }
                }

                break;

            case SpawnMode.RandomPointInRadius:
                Vector2 circle = Random.insideUnitCircle * Mathf.Max(0.1f, spawnRadius);
                Vector3 candidate = transform.position + new Vector3(circle.x, 0f, circle.y);
                if (sampleNavMesh && NavMesh.SamplePosition(candidate, out NavMeshHit hit, Mathf.Max(0.1f, navMeshSampleDistance), NavMesh.AllAreas))
                {
                    return hit.position;
                }

                return candidate;
        }

        return transform.position;
    }

    private GameObject PickPrefab()
    {
        if (mobPrefabs == null || mobPrefabs.Length == 0)
        {
            return null;
        }

        return mobPrefabs[Random.Range(0, mobPrefabs.Length)];
    }

    private void OnSpawnedMobDestroyed(GameObject spawnedMob)
    {
        aliveMobs.Remove(spawnedMob);
    }

    private void CleanupAliveList()
    {
        for (int i = aliveMobs.Count - 1; i >= 0; i--)
        {
            if (aliveMobs[i] == null)
            {
                aliveMobs.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.45f);
        if (spawnMode == SpawnMode.RandomPointInRadius)
        {
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.1f, spawnRadius));
        }

        if (spawnMode == SpawnMode.RandomFromPointList && spawnPoints != null)
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.65f);
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                {
                    Gizmos.DrawSphere(spawnPoints[i].position, 0.2f);
                }
            }
        }
    }
}
