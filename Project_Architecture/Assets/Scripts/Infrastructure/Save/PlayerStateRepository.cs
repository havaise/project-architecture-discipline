using UnityEngine;

public sealed class PlayerStateRepository : IPlayerStateRepository
{
    private readonly Transform playerTransform;
    private readonly HealthComponent playerHealth;
    private readonly ManaComponent playerMana;
    private readonly PlayerStatsComponent playerStats;

    public PlayerStateRepository(
        Transform playerTransform,
        HealthComponent playerHealth,
        ManaComponent playerMana,
        PlayerStatsComponent playerStats)
    {
        this.playerTransform = playerTransform;
        this.playerHealth = playerHealth;
        this.playerMana = playerMana;
        this.playerStats = playerStats;
    }

    public bool TryCapture(out PlayerSaveData data)
    {
        data = null;
        if (playerTransform == null)
        {
            return false;
        }

        data = new PlayerSaveData
        {
            Position = playerTransform.position,
            Rotation = playerTransform.rotation,
            CurrentHp = playerHealth != null ? playerHealth.Current : 0,
            MaxHp = playerHealth != null ? playerHealth.Max : 0,
            CurrentMp = playerMana != null ? playerMana.Current : 0,
            MaxMp = playerMana != null ? playerMana.Max : 0,
            Level = playerStats != null ? playerStats.Level : 1,
            Experience = playerStats != null ? playerStats.Experience : 0
        };

        return true;
    }

    public void Restore(PlayerSaveData data)
    {
        if (data == null || playerTransform == null)
        {
            return;
        }

        playerTransform.SetPositionAndRotation(data.Position, data.Rotation);

        if (playerHealth != null)
        {
            playerHealth.SetCurrent(data.CurrentHp);
        }

        if (playerMana != null)
        {
            playerMana.SetCurrent(data.CurrentMp);
        }

        if (playerStats != null)
        {
            playerStats.Level = data.Level;
            playerStats.Experience = data.Experience;
        }
    }
}
