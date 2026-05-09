using UnityEngine;

public readonly struct PlayerCompositionResult
{
    public PlayerCompositionResult(
        Transform playerTransform,
        PlayerView playerView,
        HealthComponent playerHealth,
        ManaComponent playerMana,
        PlayerStatsComponent playerStats)
    {
        PlayerTransform = playerTransform;
        PlayerView = playerView;
        PlayerHealth = playerHealth;
        PlayerMana = playerMana;
        PlayerStats = playerStats;
    }

    public Transform PlayerTransform { get; }
    public PlayerView PlayerView { get; }
    public HealthComponent PlayerHealth { get; }
    public ManaComponent PlayerMana { get; }
    public PlayerStatsComponent PlayerStats { get; }
}

public static class PlayerComposition
{
    private const string PlayerTag = "Player";

    public static PlayerCompositionResult Compose(
        Transform playerTransform,
        PlayerMovement playerMovement,
        PlayerView playerView,
        IInputService inputService)
    {
        Transform resolvedTransform = ResolvePlayerTransform(playerTransform, playerMovement);
        PlayerView resolvedView = EnsurePlayerView(resolvedTransform, playerView);

        if (resolvedView != null)
        {
            resolvedView.Initialize(inputService);
        }

        HealthComponent health = ResolvePlayerComponent<HealthComponent>(resolvedTransform, playerMovement);
        ManaComponent mana = ResolvePlayerComponent<ManaComponent>(resolvedTransform, playerMovement);
        PlayerStatsComponent stats = ResolvePlayerComponent<PlayerStatsComponent>(resolvedTransform, playerMovement);

        return new PlayerCompositionResult(
            resolvedTransform,
            resolvedView,
            health,
            mana,
            stats);
    }

    private static Transform ResolvePlayerTransform(Transform playerTransform, PlayerMovement playerMovement)
    {
        if (playerTransform != null)
        {
            return playerTransform;
        }

        if (playerMovement != null)
        {
            return playerMovement.transform;
        }

        PlayerMovement sceneMovement = Object.FindFirstObjectByType<PlayerMovement>();
        return sceneMovement != null ? sceneMovement.transform : null;
    }

    private static PlayerView EnsurePlayerView(Transform playerTransform, PlayerView playerView)
    {
        if (playerView != null)
        {
            return playerView;
        }

        if (playerTransform == null)
        {
            return null;
        }

        PlayerView resolvedView = playerTransform.GetComponent<PlayerView>();
        if (resolvedView == null)
        {
            resolvedView = playerTransform.gameObject.AddComponent<PlayerView>();
        }

        return resolvedView;
    }

    private static T ResolvePlayerComponent<T>(Transform playerTransform, PlayerMovement playerMovement) where T : Component
    {
        if (playerTransform != null)
        {
            T fromTransform = playerTransform.GetComponentInChildren<T>();
            if (fromTransform != null)
            {
                return fromTransform;
            }
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag(PlayerTag);
        if (taggedPlayer != null)
        {
            T fromTag = taggedPlayer.GetComponentInChildren<T>();
            if (fromTag != null)
            {
                return fromTag;
            }
        }

        return playerMovement != null ? playerMovement.GetComponentInChildren<T>() : null;
    }
}
