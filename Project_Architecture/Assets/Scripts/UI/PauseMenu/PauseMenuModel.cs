public sealed class PauseMenuModel
{
    public bool IsPaused { get; private set; }

    public void SetPaused(bool value)
    {
        IsPaused = value;
    }
}
