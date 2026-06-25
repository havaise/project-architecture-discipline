using System;
using System.IO;
using UnityEngine;

public sealed class JsonFileSaveService : ISaveService
{
    private readonly string savePath;

    public JsonFileSaveService()
    {
        savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    public void Save(SaveGameData data)
    {
        if (data == null)
        {
            Debug.LogError("JsonFileSaveService: data is null.");
            return;
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public bool TryLoad(out SaveGameData data)
    {
        data = null;

        if (!File.Exists(savePath))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveGameData loaded = JsonUtility.FromJson<SaveGameData>(json);
            if (loaded == null || string.IsNullOrWhiteSpace(loaded.SceneName))
            {
                return false;
            }

            EnsureBackwardCompatibility(loaded);

            data = loaded;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"JsonFileSaveService: failed to load save. {ex.Message}");
            return false;
        }
    }

    private static void EnsureBackwardCompatibility(SaveGameData loaded)
    {
        if (loaded.Player == null)
        {
            loaded.Player = new PlayerSaveData();
        }

        bool hasLegacyPlayerTransform =
            loaded.PlayerPosition != Vector3.zero
            || loaded.PlayerRotation != default;
        bool playerLooksEmpty =
            loaded.Player.Position == Vector3.zero
            && loaded.Player.Rotation == default
            && loaded.Player.CurrentHp == 0
            && loaded.Player.MaxHp == 0
            && loaded.Player.CurrentMp == 0
            && loaded.Player.MaxMp == 0
            && loaded.Player.Level == 0
            && loaded.Player.Experience == 0;

        if (playerLooksEmpty && hasLegacyPlayerTransform)
        {
            loaded.Player = new PlayerSaveData
            {
                Position = loaded.PlayerPosition,
                Rotation = loaded.PlayerRotation
            };
        }

        if (loaded.Player.Rotation == default)
        {
            loaded.Player.Rotation = Quaternion.identity;
        }

        if (loaded.Enemies == null)
        {
            loaded.Enemies = new System.Collections.Generic.List<EnemySaveData>();
        }
    }
}
