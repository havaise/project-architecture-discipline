public interface IHealth
{
    event System.Action<int, int> HealthChanged;
    event System.Action Died;

    int Current { get; }
    int Max { get; }

    void Reduce(int count);
    bool IsDead();
}
