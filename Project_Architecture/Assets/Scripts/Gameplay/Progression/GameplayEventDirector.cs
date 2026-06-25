using System;
using UnityEngine;

public class GameplayEventDirector : MonoBehaviour
{
    public event Action VictoryReached;

    [Header("Thresholds")]
    [SerializeField] private int bossSpawnKillThreshold = 3;
    [SerializeField] private int victoryMusicKillThreshold = 5;
    [SerializeField] private int scorePerKill = 100;

    [Header("Boss Reward")]
    [SerializeField] private int bossKillScore = 500;
    [SerializeField] private int bossKillCount = 1;

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

    public int Kills => scoreboardModel != null ? scoreboardModel.Kills : 0;
    public int Score => scoreboardModel != null ? scoreboardModel.Score : 0;

    private ScoreboardModel scoreboardModel;
    private ScoreboardController scoreboardController;
    private bool bossSpawned;
    private bool victoryPlayed;

    private void Awake()
    {
        scoreboardModel = new ScoreboardModel();
        scoreboardController = new ScoreboardController(scoreboardModel, scoreboardView);
        scoreboardController.Initialize();

        if (hideBossObjectOnStart && bossObjectToActivate != null)
        {
            bossObjectToActivate.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EnemyController.EnemyDied += OnEnemyDied;
        BossController.BossDied += OnBossDied;
        scoreboardModel?.NotifyCurrent();
    }

    private void OnDisable()
    {
        EnemyController.EnemyDied -= OnEnemyDied;
        BossController.BossDied -= OnBossDied;
    }

    private void OnDestroy()
    {
        scoreboardController?.Dispose();
    }


    public GameplayProgressSaveData CaptureProgress()
    {
        return new GameplayProgressSaveData
        {
            Score = Score,
            Kills = Kills,
            BossSpawned = bossSpawned,
            VictoryPlayed = victoryPlayed
        };
    }

    public void RestoreProgress(GameplayProgressSaveData data)
    {
        if (data == null)
        {
            return;
        }

        scoreboardModel?.SetProgress(data.Score, data.Kills);
        bossSpawned = data.BossSpawned;
        victoryPlayed = data.VictoryPlayed;

        if (bossSpawned && bossObjectToActivate != null)
        {
            bossObjectToActivate.SetActive(true);
        }
    }
    private void OnEnemyDied(EnemyController enemy)
    {
        AddProgress(1, scorePerKill);
    }

    private void OnBossDied(BossController boss)
    {
        AddProgress(Mathf.Max(0, bossKillCount), Mathf.Max(0, bossKillScore));
    }

    private void AddProgress(int killDelta, int scoreDelta)
    {
        scoreboardModel?.AddProgress(killDelta, scoreDelta);

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

}



