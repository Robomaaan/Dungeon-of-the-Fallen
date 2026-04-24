using System.Xml.Serialization;
using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Persistence
{
    [XmlRoot("SaveData")]
    public class SaveData
    {
        public int SaveVersion { get; set; } = SaveConstants.CurrentSaveVersion;
        public string PlayerName { get; set; } = "Hero";
        public int PlayerPositionX { get; set; }
        public int PlayerPositionY { get; set; }
        public int PlayerHP { get; set; }
        public int PlayerMaxHP { get; set; }
        public int PlayerAttack { get; set; }
        public int PlayerDefense { get; set; }
        public int PlayerXP { get; set; }
        public int PlayerLevel { get; set; }
        public int PlayerGold { get; set; }
        public int PotionCount { get; set; }
        public List<EnemySaveData> Enemies { get; set; } = new();
    }

    public class EnemySaveData
    {
        public string Name { get; set; } = string.Empty;
        public EnemyType EnemyType { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public bool IsBoss { get; set; }
        public int GoldReward { get; set; }
        public int XpReward { get; set; }
    }
}
