public interface ISaveService
{
    void Save(SaveGameData data);
    bool TryLoad(out SaveGameData data);
}
