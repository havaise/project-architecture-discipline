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

            data = loaded;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"JsonFileSaveService: failed to load save. {ex.Message}");
            return false;
        }
    }
}
