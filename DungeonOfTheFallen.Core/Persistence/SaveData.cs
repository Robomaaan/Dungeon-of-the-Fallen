using System.Xml.Serialization;

namespace DungeonOfTheFallen.Core.Persistence
{
    [XmlRoot("SaveData")]
    public class SaveData
    {
        public string PlayerName { get; set; } = "Hero";
        public int PlayerHP { get; set; }
        public int PlayerMaxHP { get; set; }
        public int PlayerAttack { get; set; }
        public int PlayerDefense { get; set; }
        public int PlayerXP { get; set; }
        public int PlayerLevel { get; set; }
        public int PlayerGold { get; set; }
        public int PotionCount { get; set; }
    }
}
