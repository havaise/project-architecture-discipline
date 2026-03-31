public interface IMagicCooldownProvider
{
    float MagicCooldownDuration { get; }
    float MagicCooldownRemaining { get; }
    float MagicCooldownNormalized { get; }
    bool IsMagicReady { get; }
}
