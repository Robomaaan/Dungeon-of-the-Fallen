using System.Xml.Serialization;

namespace DungeonOfTheFallen.Core.Persistence
{
    public class XmlGameRepository : IGameRepository
    {
        public void Save(SaveData data, string filename)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(SaveData));
                using var writer = new StreamWriter(filename);
                serializer.Serialize(writer, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save error: {ex.Message}");
            }
        }

        public SaveData? Load(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                    return null;

                var serializer = new XmlSerializer(typeof(SaveData));
                using var reader = new StreamReader(filename);
                return serializer.Deserialize(reader) as SaveData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load error: {ex.Message}");
                return null;
            }
        }
    }
}
