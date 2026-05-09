using System;

public sealed class StateMachine<TContext>
{
    private readonly TContext context;
    private IState<TContext> currentState;

    public StateMachine(TContext context)
    {
        this.context = context;
    }

    public string CurrentStateName => currentState != null ? currentState.Name : string.Empty;

    public void SetInitialState(IState<TContext> state)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        currentState = state;
        currentState.Enter(context);
    }

    public void Tick(float currentTime, float deltaTime)
    {
        currentState?.Tick(context, currentTime, deltaTime);
    }

    public void ChangeState(IState<TContext> nextState)
    {
        if (nextState == null || ReferenceEquals(nextState, currentState))
        {
            return;
        }

        currentState?.Exit(context);
        currentState = nextState;
        currentState.Enter(context);
    }
}
