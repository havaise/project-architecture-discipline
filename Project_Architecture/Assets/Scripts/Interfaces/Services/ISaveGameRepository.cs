public interface ISaveGameRepository
{
    void Save(SaveGameData data);
    bool TryLoad(out SaveGameData data);
}
