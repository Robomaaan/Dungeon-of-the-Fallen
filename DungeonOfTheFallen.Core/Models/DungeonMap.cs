namespace DungeonOfTheFallen.Core.Models
{
    public class DungeonMap
    {
        private readonly Tile[,] _tiles;
        public int Width { get; }
        public int Height { get; }

        public DungeonMap(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new Tile[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _tiles[x, y] = new Tile(x, y, TileType.Floor);
                }
            }
        }

        public Tile? GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return null;
            }

            return _tiles[x, y];
        }

        public void SetTile(int x, int y, Tile tile)
        {
            ArgumentNullException.ThrowIfNull(tile);

            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            _tiles[x, y] = tile;
        }

        public Tile[] GetAllTiles()
        {
            var allTiles = new List<Tile>(Width * Height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    allTiles.Add(_tiles[x, y]);
                }
            }
            return allTiles.ToArray();
        }
    }
}
