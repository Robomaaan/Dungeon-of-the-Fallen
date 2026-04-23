namespace DungeonOfTheFallen.Core.Persistence
{
    public interface IGameRepository
    {
        void Save(SaveData data, string filename);
        SaveData? Load(string filename);
    }
}
