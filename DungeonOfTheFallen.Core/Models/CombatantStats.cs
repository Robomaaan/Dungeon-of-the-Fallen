namespace DungeonOfTheFallen.Core.Models
{
    public class CombatantStats
    {
        public int MaxHP { get; set; }
        public int HP { get; set; }
        public int AttackBonus { get; set; }
        public int ArmorClass { get; set; }
        public WeaponProfile Weapon { get; set; } = new();
        public DamageModifierSet DamageModifiers { get; set; } = new();

        public int Attack
        {
            get => AttackBonus;
            set => AttackBonus = value;
        }

        public int Defense
        {
            get => ArmorClass;
            set => ArmorClass = value;
        }

        public bool IsAlive => HP > 0;

        public void RestoreToFullHealth() => HP = MaxHP;
    }
}
