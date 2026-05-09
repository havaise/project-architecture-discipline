using UnityEngine;

public interface IEnemyTargetProvider
{
    Transform Resolve(Transform currentTarget, bool autoFindPlayer);
}

public sealed class UnityEnemyTargetProvider : IEnemyTargetProvider
{
    private readonly string playerTag;

    public UnityEnemyTargetProvider(string playerTag)
    {
        this.playerTag = string.IsNullOrWhiteSpace(playerTag) ? "Player" : playerTag;
    }

    public Transform Resolve(Transform currentTarget, bool autoFindPlayer)
    {
        if (currentTarget != null)
        {
            return currentTarget;
        }

        if (!autoFindPlayer)
        {
            return null;
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag(playerTag);
        if (taggedPlayer != null)
        {
            return taggedPlayer.transform;
        }

        PlayerMovement playerMovement = Object.FindFirstObjectByType<PlayerMovement>();
        return playerMovement != null ? playerMovement.transform : null;
    }
}
