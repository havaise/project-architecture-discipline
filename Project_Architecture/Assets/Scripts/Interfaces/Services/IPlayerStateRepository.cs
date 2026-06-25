public interface IPlayerStateRepository
{
    bool TryCapture(out PlayerSaveData data);
    void Restore(PlayerSaveData data);
}
