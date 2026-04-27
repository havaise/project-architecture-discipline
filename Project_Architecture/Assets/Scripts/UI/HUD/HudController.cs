public sealed class HudController
{
    private readonly HudModel model;
    private readonly HudView view;
    private bool initialized;

    public HudController(HudModel model, HudView view)
    {
        this.model = model;
        this.view = view;
    }

    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        model.HealthChanged += OnHealthChanged;
        model.Subscribe();

        if (model.HasHealth)
        {
            view.RenderHealth(model.CurrentHealth, model.MaxHealth);
        }

        if (model.HasCooldown)
        {
            view.RenderCooldown(model.CooldownRemaining);
        }

        initialized = true;
    }

    public void Dispose()
    {
        if (!initialized)
        {
            return;
        }

        model.Unsubscribe();
        model.HealthChanged -= OnHealthChanged;
        initialized = false;
    }

    public void Tick()
    {
        if (!initialized || !model.HasCooldown)
        {
            return;
        }

        view.RenderCooldown(model.CooldownRemaining);
    }

    private void OnHealthChanged(int current, int max)
    {
        view.RenderHealth(current, max);
    }
}
