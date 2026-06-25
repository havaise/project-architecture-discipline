public interface IGameplayProgressRepository
{
    GameplayProgressSaveData Capture();
    void Restore(GameplayProgressSaveData data);
}
