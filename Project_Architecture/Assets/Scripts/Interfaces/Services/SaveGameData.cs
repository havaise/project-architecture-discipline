using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveGameData
{
    public string SceneName;
    public PlayerSaveData Player = new PlayerSaveData();
    public List<EnemySaveData> Enemies = new List<EnemySaveData>();
    public GameplayProgressSaveData Progress = new GameplayProgressSaveData();

    // Legacy fields kept for backward compatibility with older saves.
    public Vector3 PlayerPosition;
    public Quaternion PlayerRotation;
}

[Serializable]
public class PlayerSaveData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public int CurrentHp;
    public int MaxHp;
    public int CurrentMp;
    public int MaxMp;
    public int Level;
    public int Experience;
}

[Serializable]
public class GameplayProgressSaveData
{
    public int Score;
    public int Kills;
    public bool BossSpawned;
    public bool VictoryPlayed;
}

[Serializable]
public class EnemySaveData
{
    public string Id;
    public Vector3 Position;
    public Quaternion Rotation;
    public int CurrentHp;
    public int MaxHp;
}

