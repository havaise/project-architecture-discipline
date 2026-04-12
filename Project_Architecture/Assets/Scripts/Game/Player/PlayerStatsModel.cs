using UnityEngine;

public sealed class PlayerStatsModel
{
    private int level;
    private int experience;

    public int Level => level;
    public int Experience => experience;

    public void Initialize(int initialLevel, int initialExperience)
    {
        level = Mathf.Max(1, initialLevel);
        experience = Mathf.Max(0, initialExperience);
    }

    public void SetLevel(int value)
    {
        level = Mathf.Max(1, value);
    }

    public void SetExperience(int value)
    {
        experience = Mathf.Max(0, value);
    }
}
