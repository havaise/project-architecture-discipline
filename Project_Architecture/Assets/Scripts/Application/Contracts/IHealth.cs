public interface IHealth
{
    event System.Action<int, int> HealthChanged;
    event System.Action Died;

    int Current { get; }
    int Max { get; }
    int _current { get; }
    int _max { get; }

    void Reduce(int count);
    bool IsDead();
    bool isDead();
}
