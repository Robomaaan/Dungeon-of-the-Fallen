namespace DungeonOfTheFallen.Core.Models
{
    public class PlayerClassProfile
    {
        public PlayerClass PlayerClass { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxHP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int StartingPotions { get; set; }
        public string AccentColorHex { get; set; } = "#FFD700";
        public WeaponProfile StartingWeapon { get; set; } = new();
        public DamageModifierSet DamageModifiers { get; set; } = new();
        public PlayerSkill ClassSkill { get; set; } = new();
    }
}
