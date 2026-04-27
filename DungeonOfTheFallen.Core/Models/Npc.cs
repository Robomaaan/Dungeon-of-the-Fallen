namespace DungeonOfTheFallen.Core.Models
{
    public class Npc
    {
        public string Name { get; set; } = string.Empty;
        public NpcType NpcType { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public bool HasInteractedOnFloor { get; set; }
        public string Greeting { get; set; } = string.Empty;
    }
}
