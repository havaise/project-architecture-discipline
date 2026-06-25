public interface IState<TContext>
{
    string Name { get; }

    void Enter(TContext context);
    void Tick(TContext context, float currentTime, float deltaTime);
    void Exit(TContext context);
}
