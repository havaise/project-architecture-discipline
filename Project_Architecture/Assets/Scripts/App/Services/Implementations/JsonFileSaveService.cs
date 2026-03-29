using System;
using System.IO;
using UnityEngine;

public sealed class JsonFileSaveService : ISaveService
{
    private readonly string saveFilePath;

    public JsonFileSaveService()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    public void Save(SaveGameData data)
    {
        if (data == null)
        {
            Debug.LogError("JsonFileSaveService: cannot save null data.");
            return;
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
    }

    public bool TryLoad(out SaveGameData data)
    {
        data = null;

        if (!File.Exists(saveFilePath))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            SaveGameData loaded = JsonUtility.FromJson<SaveGameData>(json);
            if (loaded == null || string.IsNullOrWhiteSpace(loaded.SceneName))
            {
                return false;
            }

            data = loaded;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"JsonFileSaveService: load failed: {ex.Message}");
            return false;
        }
    }
}
