namespace DungeonOfTheFallen.Core.Models
{
    public class Item
    {
        public string Name { get; set; }
        public ItemType ItemType { get; set; }

        public Item(string name, ItemType itemType)
        {
            Name = name;
            ItemType = itemType;
        }
    }
}
