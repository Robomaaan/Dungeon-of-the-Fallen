using System.Collections.ObjectModel;
using DungeonOfTheFallen.Core.Models;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const int DungeonSize = 20;
        private const int SpawnX = 2;
        private const int SpawnY = 2;
        private const int ExitInset = 3;
        private const int InteriorWallY = 10;
        private const int InteriorWallStartX = 5;
        private const int InteriorWallEndX = 10;

        public GameState GameState { get; }
        public PlayerViewModel PlayerViewModel { get; }
        public ObservableCollection<TileViewModel> Tiles { get; } = new();

        public int DungeonWidth => GameState.Map.Width;
        public int DungeonHeight => GameState.Map.Height;

        public MainViewModel()
        {
            GameState = new GameState(DungeonSize, DungeonSize);
            PlayerViewModel = new PlayerViewModel(GameState.Player);
            InitializeDemoMap();
            RefreshTileCollection();
        }

        private void InitializeDemoMap()
        {
            var map = GameState.Map;

            FillMap(map, TileType.Floor);
            CreateBorderWalls(map);
            PlaceSpawn(map);
            PlaceExit(map);
            CreateInteriorWall(map);
        }

        public void RefreshTileCollection()
        {
            Tiles.Clear();
            for (int y = 0; y < GameState.Map.Height; y++)
            {
                for (int x = 0; x < GameState.Map.Width; x++)
                {
                    var tile = GameState.Map.GetTile(x, y);
                    if (tile is not null)
                    {
                        Tiles.Add(new TileViewModel(tile));
                    }
                }
            }
        }

        public void UpdateAllTiles()
        {
            foreach (var tileVm in Tiles)
            {
                tileVm.UpdateDisplay();
            }
            PlayerViewModel.UpdateStatus();
        }

        private static void FillMap(DungeonMap map, TileType tileType)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    SetTile(map, x, y, tileType);
                }
            }
        }

        private static void CreateBorderWalls(DungeonMap map)
        {
            for (int x = 0; x < map.Width; x++)
            {
                SetTile(map, x, 0, TileType.Wall);
                SetTile(map, x, map.Height - 1, TileType.Wall);
            }

            for (int y = 0; y < map.Height; y++)
            {
                SetTile(map, 0, y, TileType.Wall);
                SetTile(map, map.Width - 1, y, TileType.Wall);
            }
        }

        private void PlaceSpawn(DungeonMap map)
        {
            var spawnTile = map.GetTile(SpawnX, SpawnY);
            if (spawnTile is null)
            {
                return;
            }

            spawnTile.TileType = TileType.Spawn;
            spawnTile.HasPlayer = true;
            GameState.Player.PositionX = SpawnX;
            GameState.Player.PositionY = SpawnY;
        }

        private static void PlaceExit(DungeonMap map)
        {
            var exitTile = map.GetTile(map.Width - ExitInset, map.Height - ExitInset);
            if (exitTile is not null)
            {
                exitTile.TileType = TileType.Exit;
            }
        }

        private static void CreateInteriorWall(DungeonMap map)
        {
            for (int x = InteriorWallStartX; x < InteriorWallEndX; x++)
            {
                SetTile(map, x, InteriorWallY, TileType.Wall);
            }
        }

        private static void SetTile(DungeonMap map, int x, int y, TileType tileType)
        {
            map.SetTile(x, y, new Tile(x, y, tileType));
        }
    }
}
