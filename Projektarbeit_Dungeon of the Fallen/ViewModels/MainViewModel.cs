using System.Collections.ObjectModel;
using DungeonOfTheFallen.Core.Models;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    /// <summary>
    /// Zentrale ViewModel für das Spiel
    /// Verwaltet GameState, Tile-Collection und Spieler-UI
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private GameState _gameState;
        private PlayerViewModel _playerViewModel;
        private ObservableCollection<TileViewModel> _tiles;

        public GameState GameState => _gameState;
        public PlayerViewModel PlayerViewModel => _playerViewModel;
        public ObservableCollection<TileViewModel> Tiles => _tiles;

        public int DungeonWidth { get; private set; }
        public int DungeonHeight { get; private set; }

        public MainViewModel()
        {
            // Initialisiere neues Spiel
            _gameState = new GameState(20, 20);
            DungeonWidth = _gameState.Map.Width;
            DungeonHeight = _gameState.Map.Height;

            _playerViewModel = new PlayerViewModel(_gameState.Player);
            _tiles = new ObservableCollection<TileViewModel>();

            // Erstelle hardcoded Test-Map
            InitializeTestMap();

            // Fülle Tiles-Collection für UI-Binding
            RefreshTileCollection();
        }

        /// <summary>
        /// Erstellt eine Test-Map mit Wänden, Spawn und Exit
        /// </summary>
        private void InitializeTestMap()
        {
            var map = _gameState.Map;

            // Clear all tiles first (make them floors)
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    map.SetTile(x, y, new Tile(x, y, TileType.Floor));
                }
            }

            // Erstelle Umrandungswand
            for (int x = 0; x < map.Width; x++)
            {
                map.SetTile(x, 0, new Tile(x, 0, TileType.Wall));
                map.SetTile(x, map.Height - 1, new Tile(x, map.Height - 1, TileType.Wall));
            }
            for (int y = 0; y < map.Height; y++)
            {
                map.SetTile(0, y, new Tile(0, y, TileType.Wall));
                map.SetTile(map.Width - 1, y, new Tile(map.Width - 1, y, TileType.Wall));
            }

            // Spawn und Player
            var spawnTile = map.GetTile(2, 2);
            if (spawnTile != null)
            {
                spawnTile.TileType = TileType.Spawn;
                spawnTile.HasPlayer = true;
                _gameState.Player.PositionX = 2;
                _gameState.Player.PositionY = 2;
            }

            // Exit
            var exitTile = map.GetTile(map.Width - 3, map.Height - 3);
            if (exitTile != null)
            {
                exitTile.TileType = TileType.Exit;
            }

            // Ein paar Wände in der Mitte (einfaches Labyrinth)
            for (int x = 5; x < 10; x++)
            {
                var wallTile = map.GetTile(x, 10);
                if (wallTile != null)
                {
                    wallTile.TileType = TileType.Wall;
                    wallTile.IsWalkable = false;
                }
            }
        }

        /// <summary>
        /// Aktualisiert die Tiles-Collection aus der Map
        /// Wird nach jedem Spiel-Update aufgerufen
        /// </summary>
        public void RefreshTileCollection()
        {
            _tiles.Clear();
            for (int y = 0; y < _gameState.Map.Height; y++)
            {
                for (int x = 0; x < _gameState.Map.Width; x++)
                {
                    var tile = _gameState.Map.GetTile(x, y);
                    if (tile != null)
                    {
                        _tiles.Add(new TileViewModel(tile));
                    }
                }
            }
        }

        /// <summary>
        /// Aktualisiert alle Tile-ViewModels (nach Spieler-Bewegung, etc.)
        /// </summary>
        public void UpdateAllTiles()
        {
            foreach (var tileVm in _tiles)
            {
                tileVm.UpdateDisplay();
            }
            _playerViewModel.UpdateStatus();
        }
    }
}
