namespace DungeonOfTheFallen.Core.Models
{
    public class Tile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public TileType TileType { get; set; }
        public bool IsWalkable { get; set; }
        public bool IsDiscovered { get; set; }
        public bool HasPlayer { get; set; }
        public Enemy? Enemy { get; set; }
        public Item? Item { get; set; }

        public Tile(int x, int y, TileType tileType = TileType.Floor)
        {
            X = x;
            Y = y;
            TileType = tileType;
            IsWalkable = tileType != TileType.Wall;
            IsDiscovered = false;
            HasPlayer = false;
            Enemy = null;
            Item = null;
        }
    }
}
