using System;
using UnityEngine;

public class GameplayEventDirector : MonoBehaviour
{
    public event Action VictoryReached;

    [Header("Thresholds")]
    [SerializeField] private int bossSpawnKillThreshold = 3;
    [SerializeField] private int victoryMusicKillThreshold = 5;
    [SerializeField] private int scorePerKill = 100;

    [Header("Boss Spawn")]
    [SerializeField] private GameObject bossObjectToActivate;
    [SerializeField] private BossController bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private bool hideBossObjectOnStart = true;

    [Header("Victory Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip victoryMelody;

    [Header("UI")]
    [SerializeField] private ScoreboardView scoreboardView;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    public int Kills { get; private set; }
    public int Score { get; private set; }

    private bool bossSpawned;
    private bool victoryPlayed;

    private void Awake()
    {
        if (hideBossObjectOnStart && bossObjectToActivate != null)
        {
            bossObjectToActivate.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EnemyController.EnemyDied += OnEnemyDied;
        RefreshUi();
    }

    private void OnDisable()
    {
        EnemyController.EnemyDied -= OnEnemyDied;
    }

    private void OnEnemyDied(EnemyController enemy)
    {
        Kills++;
        Score += Mathf.Max(0, scorePerKill);
        RefreshUi();

        if (!bossSpawned && Kills >= Mathf.Max(1, bossSpawnKillThreshold))
        {
            SpawnBoss();
        }

        if (!victoryPlayed && Kills >= Mathf.Max(1, victoryMusicKillThreshold))
        {
            PlayVictoryMelody();
        }
    }

    private void SpawnBoss()
    {
        bossSpawned = true;

        if (bossObjectToActivate != null)
        {
            bossObjectToActivate.SetActive(true);
            if (debugLogs)
            {
                Debug.Log("[GameplayEventDirector] Boss activated after kill threshold.", this);
            }

            return;
        }

        if (bossPrefab != null && bossSpawnPoint != null)
        {
            Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
            if (debugLogs)
            {
                Debug.Log("[GameplayEventDirector] Boss spawned from prefab after kill threshold.", this);
            }

            return;
        }

        Debug.LogWarning("[GameplayEventDirector] Boss threshold reached, but no boss object/prefab is assigned.", this);
    }

    private void PlayVictoryMelody()
    {
        victoryPlayed = true;
        if (victoryMelody == null)
        {
            Debug.LogWarning("[GameplayEventDirector] Victory threshold reached, but victory melody is not assigned.", this);
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(victoryMelody);
        }
        else
        {
            AudioSource.PlayClipAtPoint(victoryMelody, transform.position);
        }

        VictoryReached?.Invoke();
    }

    private void RefreshUi()
    {
        if (scoreboardView != null)
        {
            scoreboardView.Render(Score, Kills);
        }
    }
}
