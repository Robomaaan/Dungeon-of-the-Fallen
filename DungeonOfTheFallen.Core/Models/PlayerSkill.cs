namespace DungeonOfTheFallen.Core.Models
{
    public class PlayerSkill
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AttackBonus { get; set; }
        public int BonusDamage { get; set; }
        public int BonusDamageDiceCount { get; set; }
        public DieSize BonusDamageDie { get; set; } = DieSize.D6;
        public DamageType BonusDamageType { get; set; } = DamageType.Physical;
        public int Healing { get; set; }
        public int DefenseBoost { get; set; }
        public int GoldCost { get; set; }
        public int XpReward { get; set; }
    }
}
