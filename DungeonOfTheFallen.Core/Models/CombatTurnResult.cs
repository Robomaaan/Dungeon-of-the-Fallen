namespace DungeonOfTheFallen.Core.Models
{
    public class CombatTurnResult
    {
        public int PlayerRoll { get; set; }
        public int EnemyRoll { get; set; }
        public int PlayerHpBefore { get; set; }
        public int PlayerHpAfter { get; set; }
        public int EnemyHpBefore { get; set; }
        public int EnemyHpAfter { get; set; }
        public int PlayerTotalAttackRoll { get; set; }
        public int EnemyTotalAttackRoll { get; set; }
        public int PlayerAttackBonus { get; set; }
        public int PlayerWeaponAttackBonus { get; set; }
        public int PlayerSkillAttackBonus { get; set; }
        public int EnemyAttackBonus { get; set; }
        public int EnemyWeaponAttackBonus { get; set; }
        public int PlayerDamageDealt { get; set; }
        public int EnemyDamageDealt { get; set; }
        public int HealingDone { get; set; }
        public int PostCombatHeal { get; set; }
        public int GoldReward { get; set; }
        public int XpReward { get; set; }
        public bool PlayerUsedPotion { get; set; }
        public bool PotionConsumed { get; set; }
        public bool PotionBlockedByFullHealth { get; set; }
        public bool PlayerUsedSkill { get; set; }
        public bool PlayerAttackHit { get; set; }
        public bool EnemyAttackHit { get; set; }
        public bool PlayerCritHit { get; set; }
        public bool PlayerCritMiss { get; set; }
        public bool EnemyCritHit { get; set; }
        public bool EnemyCritMiss { get; set; }
        public bool EnemyDefeated { get; set; }
        public bool PlayerDefeated { get; set; }
        public bool PlayerLeveledUp { get; set; }
        public bool EnemyTurnExecuted { get; set; }
        public string? UsedPotionName { get; set; }
        public string? UsedSkillName { get; set; }
        public string? PlayerDamageModifierText { get; set; }
        public string? EnemyDamageModifierText { get; set; }
        public int DefenseBoostGranted { get; set; }
        public List<string> Messages { get; } = new();
    }
}
