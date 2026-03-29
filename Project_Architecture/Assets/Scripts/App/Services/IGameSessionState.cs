public interface IGameSessionState
{
    void SetPendingLoadedGame(SaveGameData data);
    SaveGameData ConsumePendingLoadedGame();
}
