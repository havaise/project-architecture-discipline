public sealed class MagicCooldownModel
{
    private readonly IMagicCooldownProvider cooldownProvider;

    public MagicCooldownModel(IMagicCooldownProvider cooldownProvider)
    {
        this.cooldownProvider = cooldownProvider;
    }

    public float CooldownRemaining => cooldownProvider.MagicCooldownRemaining;
}
