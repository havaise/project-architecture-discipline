public interface IGameSessionState
{
    void SetPendingLoadedGame(SaveGameData data);
    SaveGameData PeekPendingLoadedGame();
    void ClearPendingLoadedGame();
}
