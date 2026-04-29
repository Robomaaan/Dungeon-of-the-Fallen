using System.Xml.Serialization;
using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Persistence
{
    [XmlRoot("SaveData")]
    public class SaveData
    {
        public int SaveVersion { get; set; } = SaveConstants.CurrentSaveVersion;
        public string PlayerName { get; set; } = "Held";
        public PlayerClass PlayerClass { get; set; } = PlayerClass.Warrior;
        public int CurrentFloor { get; set; } = 1;
        public BiomeType CurrentBiome { get; set; } = BiomeType.Forest;
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
        public string PlayerWeaponName { get; set; } = string.Empty;
        public int PlayerWeaponAttackBonus { get; set; }
        public int PlayerWeaponArmorClassBonus { get; set; }
        public int PlayerWeaponDamageCount { get; set; }
        public DieSize PlayerWeaponDamageDie { get; set; }
        public int PlayerWeaponDamageBonus { get; set; }
        public DamageType PlayerWeaponDamageType { get; set; }
        public int FloorObjectiveTarget { get; set; }
        public int EnemiesDefeatedOnFloor { get; set; }
        public bool BossDefeatedOnFloor { get; set; }
        public bool ExitUnlocked { get; set; }
        public List<string> CollectedKeyIds { get; set; } = new();
        public List<PuzzleSaveData> Puzzles { get; set; } = new();
        public List<EnemySaveData> Enemies { get; set; } = new();
        public List<NpcSaveData> Npcs { get; set; } = new();
    }

    public class PuzzleSaveData
    {
        public string PuzzleId { get; set; } = string.Empty;
        public bool Solved { get; set; }
    }

    public class NpcSaveData
    {
        public string Name { get; set; } = string.Empty;
        public NpcType NpcType { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public bool HasInteractedOnFloor { get; set; }
        public string Greeting { get; set; } = string.Empty;
    }

    public class EnemySaveData
    {
        public string Name { get; set; } = string.Empty;
        public EnemyType EnemyType { get; set; }
        public EnemyTier Tier { get; set; }
        public EnemyRole Role { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public bool IsBoss { get; set; }
        public int GoldReward { get; set; }
        public int XpReward { get; set; }
        public string WeaponName { get; set; } = string.Empty;
        public int WeaponAttackBonus { get; set; }
        public int WeaponArmorClassBonus { get; set; }
        public int WeaponDamageCount { get; set; }
        public DieSize WeaponDamageDie { get; set; }
        public int WeaponDamageBonus { get; set; }
        public DamageType WeaponDamageType { get; set; }
    }
}
