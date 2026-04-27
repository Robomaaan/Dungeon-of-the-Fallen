namespace DungeonOfTheFallen.Core.Models
{
    public class Tile
    {
        public int X { get; }
        public int Y { get; }

        private TileType _tileType;
        public TileType TileType
        {
            get => _tileType;
            set
            {
                _tileType = value;
                IsWalkable = value != TileType.Wall && value != TileType.LockedDoor;
            }
        }

        public bool IsWalkable { get; set; }
        public bool IsDiscovered { get; set; }
        public bool HasPlayer { get; set; }
        public Enemy? Enemy { get; set; }
        public Item? Item { get; set; }
        public Npc? Npc { get; set; }
        public string? DoorKeyId { get; set; }
        public string? PuzzleId { get; set; }
        public string? HintText { get; set; }

        public Tile(int x, int y, TileType tileType = TileType.Floor)
        {
            X = x;
            Y = y;
            TileType = tileType;
        }
    }
}
