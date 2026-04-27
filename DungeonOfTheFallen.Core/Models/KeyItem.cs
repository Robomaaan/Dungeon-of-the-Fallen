namespace DungeonOfTheFallen.Core.Models
{
    public class KeyItem : Item
    {
        public string KeyId { get; set; }

        public KeyItem(string name, string keyId)
            : base(name, ItemType.Key)
        {
            KeyId = keyId;
        }
    }
}
