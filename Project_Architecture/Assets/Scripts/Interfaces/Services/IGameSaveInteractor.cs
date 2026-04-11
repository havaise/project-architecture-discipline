public interface IGameSaveInteractor
{
    bool SaveCurrentGame();
    bool LoadGame();
    bool ApplyPendingLoadedGame();
}
