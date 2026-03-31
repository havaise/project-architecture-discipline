public sealed class MagicCooldownController
{
    private readonly MagicCooldownModel model;
    private readonly MagicCooldownView view;

    public MagicCooldownController(MagicCooldownModel model, MagicCooldownView view)
    {
        this.model = model;
        this.view = view;
    }

    public void Tick()
    {
        view.Render(model.CooldownRemaining);
    }
}
