using System.Collections.ObjectModel;
using System.Windows.Input;
using DungeonOfTheFallen.Core.Models;
using DungeonOfTheFallen.Core.Persistence;
using DungeonOfTheFallen.Core.Services;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const int DungeonSize = 20;
        private const int CombatLogMaxEntries = 8;
        private const int SpawnX = 2;
        private const int SpawnY = 2;
        private const int ExitX = 17;
        private const int ExitY = 17;
        private const string SaveFileName = "dungeon_save.xml";

        private TurnManager _turnManager;
        private readonly XmlGameRepository _repository = new XmlGameRepository();
        private string _combatLog = string.Empty;
        private bool _isGameOver;
        private bool _isVictory;
        private ICommand? _moveUpCommand;
        private ICommand? _moveDownCommand;
        private ICommand? _moveLeftCommand;
        private ICommand? _moveRightCommand;
        private ICommand? _restartGameCommand;
        private ICommand? _usePotionCommand;
        private ICommand? _saveGameCommand;
        private ICommand? _loadGameCommand;

        public GameState GameState { get; private set; }
        public PlayerViewModel PlayerViewModel { get; private set; }
        public ObservableCollection<TileViewModel> Tiles { get; } = new();

        public int DungeonWidth => GameState.Map.Width;
        public int DungeonHeight => GameState.Map.Height;

        public string CombatLog
        {
            get => _combatLog;
            set => SetProperty(ref _combatLog, value);
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            set => SetProperty(ref _isGameOver, value);
        }

        public bool IsVictory
        {
            get => _isVictory;
            set => SetProperty(ref _isVictory, value);
        }

        // View subscribes hier und öffnet das CombatWindow (kein View-Code im ViewModel)
        public event Action<Enemy>? CombatRequested;

        public ICommand MoveUpCommand => _moveUpCommand ??= new RelayCommand(_ => MovePlayer(0, -1));
        public ICommand MoveDownCommand => _moveDownCommand ??= new RelayCommand(_ => MovePlayer(0, 1));
        public ICommand MoveLeftCommand => _moveLeftCommand ??= new RelayCommand(_ => MovePlayer(-1, 0));
        public ICommand MoveRightCommand => _moveRightCommand ??= new RelayCommand(_ => MovePlayer(1, 0));
        public ICommand RestartGameCommand => _restartGameCommand ??= new RelayCommand(_ => RestartGame());
        public ICommand UsePotionCommand => _usePotionCommand ??= new RelayCommand(_ => UsePotion(), _ => !IsGameOver);
        public ICommand SaveGameCommand => _saveGameCommand ??= new RelayCommand(_ => SaveGame(), _ => !IsGameOver);
        public ICommand LoadGameCommand => _loadGameCommand ??= new RelayCommand(_ => LoadGame());

        public MainViewModel()
        {
            GameState = new GameState(DungeonSize, DungeonSize);
            _turnManager = new TurnManager(GameState);
            PlayerViewModel = new PlayerViewModel(GameState.Player);

            InitializeDemoMap();
            SpawnEnemies();
            RefreshTileCollection();
            UpdateCombatLog("Game started! WASD / Arrow Keys = Move | P = Potion | F5 = Save | F9 = Load");
        }

        private void MovePlayer(int dx, int dy)
        {
            if (IsGameOver || IsVictory) return;

            int newX = GameState.Player.PositionX + dx;
            int newY = GameState.Player.PositionY + dy;

            if (newX < 0 || newX >= DungeonSize || newY < 0 || newY >= DungeonSize) return;

            var targetTile = GameState.Map.GetTile(newX, newY);
            if (targetTile == null || !targetTile.IsWalkable) return;

            // Gegner auf Ziel-Tile: Kampfscreen öffnen
            if (targetTile.Enemy != null && targetTile.Enemy.IsAlive)
            {
                var enemy = targetTile.Enemy;
                OpenCombat(enemy);

                UpdateAllTiles();
                UpdateCombatLog();
                CheckGameConditions();

                // Wenn Gegner besiegt: Spieler betritt Feld
                if (!IsGameOver && !enemy.IsAlive)
                {
                    _turnManager.MovePlayer(newX, newY);
                    UpdateAllTiles();
                    UpdateCombatLog();
                    CheckGameConditions();
                }
                return;
            }

            bool moved = _turnManager.MovePlayer(newX, newY);
            if (moved)
            {
                UpdateAllTiles();
                UpdateCombatLog();
                CheckGameConditions();
            }
        }

        private void OpenCombat(Enemy enemy)
        {
            // Event auslösen → MainWindow.xaml.cs öffnet das CombatWindow
            CombatRequested?.Invoke(enemy);
        }

        private void UsePotion()
        {
            if (IsGameOver || IsVictory) return;

            var potion = GameState.Player.Inventory.Items.OfType<Potion>().FirstOrDefault();
            if (potion == null)
            {
                GameState.AddCombatLogEntry("[INFO] No potions in inventory!");
                UpdateCombatLog();
                return;
            }

            int heal = Math.Min(potion.HealingAmount, GameState.Player.MaxHP - GameState.Player.HP);
            GameState.Player.HP += heal;
            GameState.Player.Inventory.Remove(potion);
            GameState.AddCombatLogEntry($"[HEAL] Used {potion.Name}: +{heal} HP restored!");
            UpdateAllTiles();
            UpdateCombatLog();
        }

        private void SaveGame()
        {
            var data = SaveDataMapper.ToSaveData(GameState);
            _repository.Save(data, SaveFileName);
            GameState.AddCombatLogEntry("[SAVE] Game saved successfully!");
            UpdateCombatLog();
        }

        private void LoadGame()
        {
            var data = _repository.Load(SaveFileName);
            if (data == null)
            {
                GameState.AddCombatLogEntry("[LOAD] No save file found!");
                UpdateCombatLog();
                return;
            }

            IsGameOver = false;
            IsVictory = false;
            SaveDataMapper.ApplyToGameState(GameState, data);

            GameState.AddCombatLogEntry($"[LOAD] Game loaded! Save v{data.SaveVersion} restored.");
            UpdateAllTiles();
            UpdateCombatLog();
            CheckGameConditions();
        }

        private void SpawnEnemies()
        {
            var random = new Random();
            var positions = GetValidEnemySpawnPositions();

            if (positions.Count > 0)
            {
                var pos = positions[random.Next(positions.Count)];
                var goblin = EnemyFactory.CreateGoblin();
                goblin.PositionX = pos.x;
                goblin.PositionY = pos.y;
                GameState.Enemies.Add(goblin);
                var tile = GameState.Map.GetTile(pos.x, pos.y);
                if (tile != null) tile.Enemy = goblin;
                positions.Remove(pos);
            }

            if (positions.Count > 0)
            {
                var pos = positions[random.Next(positions.Count)];
                var goblin2 = EnemyFactory.CreateGoblin();
                goblin2.PositionX = pos.x;
                goblin2.PositionY = pos.y;
                GameState.Enemies.Add(goblin2);
                var tile = GameState.Map.GetTile(pos.x, pos.y);
                if (tile != null) tile.Enemy = goblin2;
                positions.Remove(pos);
            }

            if (positions.Count > 0)
            {
                var pos = positions[random.Next(positions.Count)];
                var orc = EnemyFactory.CreateOrc();
                orc.PositionX = pos.x;
                orc.PositionY = pos.y;
                GameState.Enemies.Add(orc);
                var tile = GameState.Map.GetTile(pos.x, pos.y);
                if (tile != null) tile.Enemy = orc;
                positions.Remove(pos);
            }

            if (positions.Count > 0)
            {
                // Boss bewacht den Ausgang
                var bossPos = positions
                    .OrderBy(p => Math.Abs(p.x - ExitX) + Math.Abs(p.y - ExitY))
                    .First();
                var boss = EnemyFactory.CreateBoss();
                boss.PositionX = bossPos.x;
                boss.PositionY = bossPos.y;
                GameState.Enemies.Add(boss);
                var tile = GameState.Map.GetTile(bossPos.x, bossPos.y);
                if (tile != null) tile.Enemy = boss;
            }
        }

        private List<(int x, int y)> GetValidEnemySpawnPositions()
        {
            var positions = new List<(int x, int y)>();
            var map = GameState.Map;

            for (int y = 4; y < map.Height - 2; y++)
            {
                for (int x = 4; x < map.Width - 2; x++)
                {
                    // Nicht zu nah am Spawn
                    if (Math.Abs(x - SpawnX) + Math.Abs(y - SpawnY) < 6) continue;

                    var tile = map.GetTile(x, y);
                    if (tile != null && tile.IsWalkable && !tile.HasPlayer && tile.Enemy == null
                        && tile.TileType != TileType.Exit)
                    {
                        positions.Add((x, y));
                    }
                }
            }

            return positions;
        }

        private void CheckGameConditions()
        {
            if (_turnManager.CheckExitReached())
            {
                IsVictory = true;
                IsGameOver = true;
                return;
            }

            if (!GameState.Player.IsAlive)
            {
                IsGameOver = true;
            }
        }

        private void RestartGame()
        {
            GameState = new GameState(DungeonSize, DungeonSize);
            _turnManager = new TurnManager(GameState);
            PlayerViewModel = new PlayerViewModel(GameState.Player);
            OnPropertyChanged(nameof(GameState), nameof(PlayerViewModel), nameof(DungeonWidth), nameof(DungeonHeight));

            IsGameOver = false;
            IsVictory = false;

            InitializeDemoMap();
            SpawnEnemies();
            RefreshTileCollection();
            UpdateAllTiles();
            UpdateCombatLog("Game restarted. Good luck, hero!");
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
                        Tiles.Add(new TileViewModel(tile));
                }
            }
        }

        public void UpdateAllTiles()
        {
            foreach (var tileVm in Tiles)
                tileVm.UpdateDisplay();
            PlayerViewModel.UpdateStatus();
        }

        private void UpdateCombatLog(string? overrideMessage = null)
        {
            if (overrideMessage != null)
            {
                CombatLog = overrideMessage;
                return;
            }

            if (GameState.CombatLog.Count > 0)
                CombatLog = string.Join("\n", GameState.CombatLog.TakeLast(CombatLogMaxEntries));
        }

        private void InitializeDemoMap()
        {
            var map = GameState.Map;

            FillMap(map, TileType.Floor);
            CreateBorderWalls(map);

            // Linker Korridor oben (horizontal)
            for (int x = 4; x <= 10; x++) SetTile(map, x, 6, TileType.Wall);
            // Lücke bleibt bei x=10 offen (Verbindung)

            // Oberer Korridor rechts (vertikal)
            for (int y = 1; y <= 8; y++) SetTile(map, 13, y, TileType.Wall);
            // Lücke bei y=6 (Durchgang)
            SetTile(map, 13, 6, TileType.Floor);

            // Mittlere Trennwand (horizontal)
            for (int x = 1; x <= 7; x++) SetTile(map, x, 12, TileType.Wall);
            for (int x = 9; x <= 13; x++) SetTile(map, x, 12, TileType.Wall);
            // Lücke bei x=8 (Durchgang durch Mitte)

            // Rechte Kammer unten (vertikal)
            for (int y = 12; y <= 18; y++) SetTile(map, 13, y, TileType.Wall);
            // Lücke bei y=15 (Eingang zur Bosszone)
            SetTile(map, 13, 15, TileType.Floor);

            // Sonder-Tiles
            // Heilungsraum oben rechts
            var healTile = map.GetTile(16, 4);
            if (healTile != null) healTile.TileType = TileType.HealingRoom;

            // Fallen im unteren Bereich
            var trap1 = map.GetTile(5, 15);
            if (trap1 != null) trap1.TileType = TileType.Trap;
            var trap2 = map.GetTile(9, 16);
            if (trap2 != null) trap2.TileType = TileType.Trap;

            // Items auf der Map platzieren
            var potionTile = map.GetTile(10, 3);
            if (potionTile != null) potionTile.Item = new Potion("Health Potion", 25);

            var goldTile = map.GetTile(7, 9);
            if (goldTile != null) goldTile.Item = new Item("Gold Pile", ItemType.Gold);

            // Spawn und Exit
            var spawnTile = map.GetTile(SpawnX, SpawnY);
            if (spawnTile != null)
            {
                spawnTile.TileType = TileType.Spawn;
                spawnTile.HasPlayer = true;
            }
            GameState.Player.PositionX = SpawnX;
            GameState.Player.PositionY = SpawnY;

            var exitTile = map.GetTile(ExitX, ExitY);
            if (exitTile != null) exitTile.TileType = TileType.Exit;
        }

        private static void FillMap(DungeonMap map, TileType tileType)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    map.SetTile(x, y, new Tile(x, y, tileType));
        }

        private static void CreateBorderWalls(DungeonMap map)
        {
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
        }

        private static void SetTile(DungeonMap map, int x, int y, TileType tileType)
        {
            map.SetTile(x, y, new Tile(x, y, tileType));
        }
    }
}
