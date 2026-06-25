public sealed class SaveGameRepository : ISaveGameRepository
{
    private readonly ISaveService saveService;

    public SaveGameRepository(ISaveService saveService)
    {
        this.saveService = saveService;
    }

    public void Save(SaveGameData data)
    {
        saveService.Save(data);
    }

    public bool TryLoad(out SaveGameData data)
    {
        return saveService.TryLoad(out data);
    }
}
