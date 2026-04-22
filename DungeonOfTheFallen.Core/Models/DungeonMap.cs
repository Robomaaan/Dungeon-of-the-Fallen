namespace DungeonOfTheFallen.Core.Models
{
    public class DungeonMap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        private Tile[,] tiles;

        public DungeonMap(int width, int height)
        {
            Width = width;
            Height = height;
            tiles = new Tile[width, height];

            // Initialize all tiles as floors
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles[x, y] = new Tile(x, y, TileType.Floor);
                }
            }
        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null!;
            return tiles[x, y];
        }

        public void SetTile(int x, int y, Tile tile)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;
            tiles[x, y] = tile;
        }

        public Tile[] GetAllTiles()
        {
            var allTiles = new List<Tile>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    allTiles.Add(tiles[x, y]);
                }
            }
            return allTiles.ToArray();
        }
    }
}
