public sealed class GameSessionState : IGameSessionState
{
    // Static storage keeps pending load data even if services are recreated during scene switch.
    private static SaveGameData pendingLoadedGame;

    public void SetPendingLoadedGame(SaveGameData data)
    {
        pendingLoadedGame = data;
    }

    public SaveGameData PeekPendingLoadedGame()
    {
        return pendingLoadedGame;
    }

    public void ClearPendingLoadedGame()
    {
        pendingLoadedGame = null;
    }
}
