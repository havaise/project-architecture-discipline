public sealed class GameSessionState : IGameSessionState
{
    private SaveGameData pendingLoadedGame;

    public void SetPendingLoadedGame(SaveGameData data)
    {
        pendingLoadedGame = data;
    }

    public SaveGameData ConsumePendingLoadedGame()
    {
        SaveGameData data = pendingLoadedGame;
        pendingLoadedGame = null;
        return data;
    }
}
