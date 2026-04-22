namespace DungeonOfTheFallen.Core.Models
{
    public class Inventory
    {
        private readonly List<Item> _items = new();

        public IReadOnlyList<Item> Items => _items;

        public void Add(Item item)
        {
            _items.Add(item);
        }

        public bool Remove(Item item)
        {
            return _items.Remove(item);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
