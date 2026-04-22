namespace DungeonOfTheFallen.Core.Models
{
    public class Potion : Item
    {
        public int HealingAmount { get; set; }

        public Potion(string name, int healingAmount)
            : base(name, ItemType.Potion)
        {
            HealingAmount = healingAmount;
        }
    }
}
