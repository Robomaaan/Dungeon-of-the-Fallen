namespace DungeonOfTheFallen.Core.Models
{
    public class WeaponProfile
    {
        public string Name { get; set; } = "Fists";
        public int AttackBonus { get; set; }
        public DamageRoll Damage { get; set; } = new() { Count = 1, DieSize = DieSize.D4, Bonus = 0, DamageType = DamageType.Bludgeoning };
        public int ArmorClassBonus { get; set; }

        public string Summary => $"{Name} · +{AttackBonus} to hit · {Damage.Describe()}";
    }
}
