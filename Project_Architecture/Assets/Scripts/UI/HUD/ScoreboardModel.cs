using System;
using UnityEngine;

public sealed class ScoreboardModel
{
    public event Action<int, int> Changed;

    public int Score { get; private set; }
    public int Kills { get; private set; }

    public void AddProgress(int killDelta, int scoreDelta)
    {
        Kills += Mathf.Max(0, killDelta);
        Score += Mathf.Max(0, scoreDelta);
        Changed?.Invoke(Score, Kills);
    }

    public void SetProgress(int score, int kills)
    {
        Score = Mathf.Max(0, score);
        Kills = Mathf.Max(0, kills);
        Changed?.Invoke(Score, Kills);
    }

    public void NotifyCurrent()
    {
        Changed?.Invoke(Score, Kills);
    }
}

