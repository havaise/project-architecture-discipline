public sealed class GameplayProgressRepository : IGameplayProgressRepository
{
    private readonly GameplayEventDirector gameplayEventDirector;

    public GameplayProgressRepository(GameplayEventDirector gameplayEventDirector)
    {
        this.gameplayEventDirector = gameplayEventDirector;
    }

    public GameplayProgressSaveData Capture()
    {
        return gameplayEventDirector != null
            ? gameplayEventDirector.CaptureProgress()
            : new GameplayProgressSaveData();
    }

    public void Restore(GameplayProgressSaveData data)
    {
        gameplayEventDirector?.RestoreProgress(data);
    }
}
