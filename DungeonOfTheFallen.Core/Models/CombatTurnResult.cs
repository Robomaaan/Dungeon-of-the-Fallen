namespace DungeonOfTheFallen.Core.Models
{
    public class CombatTurnResult
    {
        public int PlayerRoll { get; set; }
        public int EnemyRoll { get; set; }
        public int PlayerDamageDealt { get; set; }
        public int EnemyDamageDealt { get; set; }
        public int HealingDone { get; set; }
        public int GoldReward { get; set; }
        public int XpReward { get; set; }
        public bool PlayerUsedPotion { get; set; }
        public bool EnemyDefeated { get; set; }
        public bool PlayerDefeated { get; set; }
        public bool PlayerLeveledUp { get; set; }
        public string? UsedPotionName { get; set; }
        public List<string> Messages { get; } = new();
    }
}
