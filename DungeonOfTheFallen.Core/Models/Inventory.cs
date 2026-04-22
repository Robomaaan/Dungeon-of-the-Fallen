namespace DungeonOfTheFallen.Core.Models
{
    public class Inventory
    {
        private List<Item> items = new();

        public IReadOnlyList<Item> Items => items.AsReadOnly();

        public void Add(Item item)
        {
            items.Add(item);
        }

        public bool Remove(Item item)
        {
            return items.Remove(item);
        }

        public void Clear()
        {
            items.Clear();
        }
    }
}
