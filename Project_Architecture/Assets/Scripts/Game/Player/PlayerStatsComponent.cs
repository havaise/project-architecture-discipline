using UnityEngine;

public class PlayerStatsComponent : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private int experience;

    private readonly PlayerStatsModel model = new PlayerStatsModel();

    public int Level
    {
        get => model.Level;
        set
        {
            model.SetLevel(value);
            level = model.Level;
        }
    }

    public int Experience
    {
        get => model.Experience;
        set
        {
            model.SetExperience(value);
            experience = model.Experience;
        }
    }

    private void Awake()
    {
        model.Initialize(level, experience);
        level = model.Level;
        experience = model.Experience;
    }
}
