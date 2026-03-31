using UnityEngine;

public static class EntryPointBootstrap
{
    public static void EnsureGameEntryPoint()
    {
        if (GameEntryPoint.Services != null)
        {
            return;
        }

        GameEntryPoint existing = Object.FindFirstObjectByType<GameEntryPoint>();
        if (existing != null)
        {
            return;
        }

        GameObject bootstrap = new GameObject("GameEntryPoint");
        bootstrap.AddComponent<GameEntryPoint>();
    }
}
