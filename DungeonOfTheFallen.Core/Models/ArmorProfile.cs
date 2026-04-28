namespace DungeonOfTheFallen.Core.Models
{
    public class ArmorProfile
    {
        public string Name { get; set; } = string.Empty;
        public int ArmorValue { get; set; }
        public EquipmentSlot Slot { get; set; } = EquipmentSlot.Chest;
        public string Description { get; set; } = string.Empty;

        public string Summary => $"{Name} (+{ArmorValue} RK)";
    }
}
