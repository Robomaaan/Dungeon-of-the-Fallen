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
        private const int CombatLogMaxEntries = 12;
        private const int SpawnX = 2;
        private const int SpawnY = 2;
        private const int ExitX = 17;
        private const int ExitY = 17;
        private const string SaveFileName = "dungeon_save.xml";

        private TurnManager _turnManager;
        private readonly XmlGameRepository _repository = new();
        private string _combatLog = string.Empty;
        private string _biomeDisplayName = string.Empty;
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
        public BiomeType CurrentBiome { get; private set; }
        public int CurrentFloor => GameState.CurrentFloor;
        public string FloorDisplay => $"Floor {GameState.CurrentFloor}/{GameState.FinalFloor}";
        public string ObjectiveText => GameState.ExitUnlocked
            ? "Objective complete: reach the exit."
            : $"Objective: clear the floor and slay the boss ({GameState.EnemiesDefeatedOnFloor}/{GameState.FloorObjectiveTarget}).";
        public string ExitStatusText => GameState.ExitUnlocked
            ? "Exit unlocked"
            : $"Exit sealed · Keys {GameState.CollectedKeyIds.Count} · Foes remaining {GameState.RemainingFloorEnemies}";
        public string DungeonSystemsText => $"Keys: {string.Join(", ", GameState.CollectedKeyIds.DefaultIfEmpty("none"))} · Puzzles solved: {GameState.Puzzles.Count(p => p.Solved)}/{GameState.Puzzles.Count}";
        public string RunStatusText => IsVictory
            ? "All floors conquered."
            : IsGameOver
                ? "The expedition has failed."
                : $"{BiomeDisplayName} · Level {GameState.Player.Level} · {GameState.Player.Weapon.Summary}";

        public int DungeonWidth => GameState.Map.Width;
        public int DungeonHeight => GameState.Map.Height;
        public string BiomeDisplayName
        {
            get => _biomeDisplayName;
            private set => SetProperty(ref _biomeDisplayName, value);
        }

        public string CombatLog
        {
            get => _combatLog;
            set => SetProperty(ref _combatLog, value);
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            set
            {
                if (_isGameOver == value) return;
                SetProperty(ref _isGameOver, value);
                OnPropertyChanged(nameof(RunStatusText));
            }
        }

        public bool IsVictory
        {
            get => _isVictory;
            set
            {
                if (_isVictory == value) return;
                SetProperty(ref _isVictory, value);
                OnPropertyChanged(nameof(RunStatusText));
            }
        }

        public event Action<Enemy>? CombatRequested;

        public ICommand MoveUpCommand => _moveUpCommand ??= new RelayCommand(_ => MovePlayer(0, -1));
        public ICommand MoveDownCommand => _moveDownCommand ??= new RelayCommand(_ => MovePlayer(0, 1));
        public ICommand MoveLeftCommand => _moveLeftCommand ??= new RelayCommand(_ => MovePlayer(-1, 0));
        public ICommand MoveRightCommand => _moveRightCommand ??= new RelayCommand(_ => MovePlayer(1, 0));
        public ICommand RestartGameCommand => _restartGameCommand ??= new RelayCommand(_ => RestartGame());
        public ICommand UsePotionCommand => _usePotionCommand ??= new RelayCommand(_ => UsePotion(), _ => !IsGameOver);
        public ICommand SaveGameCommand => _saveGameCommand ??= new RelayCommand(_ => SaveGame(), _ => !IsGameOver);
        public ICommand LoadGameCommand => _loadGameCommand ??= new RelayCommand(_ => LoadGame());

        public MainViewModel(PlayerClass selectedClass = PlayerClass.Warrior)
        {
            GameState = new GameState(DungeonSize, DungeonSize);
            _turnManager = new TurnManager(GameState);
            PlayerViewModel = new PlayerViewModel(GameState.Player);

            ApplyPlayerClass(selectedClass);
            BuildFloor(1, resetProgress: true);
            RefreshTileCollection();
            UpdateCombatLog($"Game started as {GameState.Player.PlayerClass}! {FloorDisplay} | {BiomeDisplayName} | Find keys, solve riddles, clear the boss chamber.");
        }

        private void MovePlayer(int dx, int dy)
        {
            if (IsGameOver || IsVictory) return;

            var newX = GameState.Player.PositionX + dx;
            var newY = GameState.Player.PositionY + dy;
            if (newX < 0 || newX >= DungeonSize || newY < 0 || newY >= DungeonSize) return;

            var targetTile = GameState.Map.GetTile(newX, newY);
            if (targetTile == null || (!targetTile.IsWalkable && targetTile.TileType != TileType.LockedDoor)) return;

            if (targetTile.TileType == TileType.Exit && !GameState.ExitUnlocked)
            {
                GameState.AddCombatLogEntry($"[EXIT] The gate is sealed. Defeat the boss and clear the floor first.");
                UpdateCombatLog();
                return;
            }

            if (targetTile.Enemy != null && targetTile.Enemy.IsAlive)
            {
                var enemy = targetTile.Enemy;
                OpenCombat(enemy);
                UpdateAllTiles();
                UpdateCombatLog();
                CheckGameConditions();

                if (!IsGameOver && !enemy.IsAlive)
                {
                    _turnManager.MovePlayer(newX, newY);
                    UpdateAllTiles();
                    UpdateCombatLog();
                    CheckGameConditions();
                }
                return;
            }

            if (_turnManager.MovePlayer(newX, newY))
            {
                UpdateAllTiles();
                UpdateCombatLog();
                CheckGameConditions();
            }
            else
            {
                UpdateCombatLog();
            }
        }

        private void OpenCombat(Enemy enemy) => CombatRequested?.Invoke(enemy);

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

            var heal = Math.Min(potion.HealingAmount, GameState.Player.MaxHP - GameState.Player.HP);
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
            GameState.AddCombatLogEntry($"[SAVE] {FloorDisplay} in {BiomeDisplayName} saved successfully!");
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

            BuildFloor(data.CurrentFloor, resetProgress: false);
            IsGameOver = false;
            IsVictory = false;
            SaveDataMapper.ApplyToGameState(GameState, data);
            CurrentBiome = GameState.CurrentBiome;
            UpdateBiomeDisplay();
            OnPropertyChanged(nameof(CurrentFloor), nameof(FloorDisplay));

            GameState.AddCombatLogEntry($"[LOAD] Game loaded! Save v{data.SaveVersion} restored on {FloorDisplay}.");
            UpdateAllTiles();
            UpdateCombatLog();
            CheckGameConditions();
        }

        private void CheckGameConditions()
        {
            if (_turnManager.CheckExitReached())
            {
                if (!GameState.ExitUnlocked)
                {
                    GameState.AddCombatLogEntry("[EXIT] The path downward remains sealed.");
                    UpdateCombatLog();
                    return;
                }

                if (GameState.CurrentFloor >= GameState.FinalFloor)
                {
                    IsVictory = true;
                    IsGameOver = true;
                    GameState.AddCombatLogEntry("[VICTORY] You conquered all dungeon floors!");
                }
                else
                {
                    AdvanceToNextFloor();
                }
                UpdateCombatLog();
                return;
            }

            if (!GameState.Player.IsAlive)
                IsGameOver = true;
        }

        private void AdvanceToNextFloor()
        {
            var nextFloor = GameState.CurrentFloor + 1;
            var heal = Math.Min(12 + (GameState.CurrentFloor * 4), GameState.Player.MaxHP - GameState.Player.HP);
            if (heal > 0)
                GameState.Player.HP += heal;

            GameState.Player.Inventory.Add(new Potion("Travel Potion", 20 + nextFloor * 4));
            BuildFloor(nextFloor, resetProgress: false);
            RefreshTileCollection();
            UpdateAllTiles();
            GameState.AddCombatLogEntry($"[FLOOR] Descended to {FloorDisplay}. New biome: {BiomeDisplayName}. Camp prep restores {heal} HP.");
        }

        private void RestartGame()
        {
            var playerClass = GameState.Player.PlayerClass;
            GameState = new GameState(DungeonSize, DungeonSize);
            _turnManager = new TurnManager(GameState);
            PlayerViewModel = new PlayerViewModel(GameState.Player);
            OnPropertyChanged(nameof(GameState), nameof(PlayerViewModel), nameof(DungeonWidth), nameof(DungeonHeight));

            ApplyPlayerClass(playerClass);
            BuildFloor(1, resetProgress: true);
            IsGameOver = false;
            IsVictory = false;

            RefreshTileCollection();
            UpdateAllTiles();
            UpdateCombatLog($"Game restarted as {GameState.Player.PlayerClass} on {FloorDisplay} in {BiomeDisplayName}.");
        }

        public void RefreshTileCollection()
        {
            Tiles.Clear();
            for (var y = 0; y < GameState.Map.Height; y++)
            for (var x = 0; x < GameState.Map.Width; x++)
            {
                var tile = GameState.Map.GetTile(x, y);
                if (tile is not null)
                    Tiles.Add(new TileViewModel(tile));
            }
        }

        public void UpdateAllTiles()
        {
            foreach (var tileVm in Tiles)
                tileVm.UpdateDisplay();
            PlayerViewModel.UpdateStatus();
            OnPropertyChanged(nameof(CurrentFloor), nameof(FloorDisplay), nameof(ObjectiveText), nameof(ExitStatusText), nameof(RunStatusText), nameof(DungeonSystemsText));
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

        private void BuildFloor(int floor, bool resetProgress)
        {
            if (resetProgress)
            {
                GameState.Player.XP = 0;
                GameState.Player.Level = 1;
                GameState.Player.Gold = 0;
                GameState.CollectedKeyIds.Clear();
            }

            GameState.CurrentFloor = floor;
            GameState.CurrentBiome = FloorThemeService.GetBiomeForFloor(floor);
            GameState.ExitUnlocked = false;
            GameState.BossDefeatedOnFloor = false;
            GameState.EnemiesDefeatedOnFloor = 0;
            CurrentBiome = GameState.CurrentBiome;
            UpdateBiomeDisplay();

            GameState.Enemies.Clear();
            GameState.Npcs.Clear();
            GameState.Puzzles.Clear();
            foreach (var tile in GameState.Map.GetAllTiles())
            {
                tile.HasPlayer = false;
                tile.Enemy = null;
                tile.Item = null;
                tile.Npc = null;
                tile.DoorKeyId = null;
                tile.PuzzleId = null;
                tile.HintText = null;
            }

            InitializeDemoMap();
            SpawnEnemies();
            SpawnNpcs();
        }

        private void SpawnEnemies()
        {
            var positions = GetValidSpawnPositions();
            var roster = EnemySpawnService.CreateRoster(CurrentBiome, GameState.CurrentFloor).ToList();
            GameState.FloorObjectiveTarget = roster.Count;

            foreach (var enemy in roster.Where(e => !e.IsBoss))
            {
                if (positions.Count == 0) break;
                var pos = positions[0];
                positions.RemoveAt(0);
                enemy.PositionX = pos.x;
                enemy.PositionY = pos.y;
                GameState.Enemies.Add(enemy);
                var tile = GameState.Map.GetTile(pos.x, pos.y);
                if (tile != null) tile.Enemy = enemy;
            }

            var boss = roster.FirstOrDefault(e => e.IsBoss);
            if (boss != null && positions.Count > 0)
            {
                var bossPos = positions.OrderByDescending(p => Math.Abs(p.x - SpawnX) + Math.Abs(p.y - SpawnY)).First();
                boss.PositionX = bossPos.x;
                boss.PositionY = bossPos.y;
                GameState.Enemies.Add(boss);
                var tile = GameState.Map.GetTile(bossPos.x, bossPos.y);
                if (tile != null) tile.Enemy = boss;
            }
        }

        private void SpawnNpcs()
        {
            var npcDefinitions = new[]
            {
                new Npc { Name = "Sister Mirel", NpcType = NpcType.Healer, Greeting = "Ruhe deinen Geist aus." },
                new Npc { Name = "Dorin", NpcType = NpcType.Merchant, Greeting = "Vorräte gegen Gold oder Mut gegen Armut." },
                new Npc { Name = "Archivist Oren", NpcType = NpcType.Chronicler, Greeting = "Hinter jedem Siegel wartet eine Geschichte." },
                new Npc { Name = "Brakka", NpcType = NpcType.Blacksmith, Greeting = "Zeig mir deine Klinge." },
                new Npc { Name = "Lysa", NpcType = NpcType.Scout, Greeting = "Ich beobachte jede Bewegung hier unten." }
            }.ToList();

            if (GameState.CurrentFloor >= 3)
                npcDefinitions.Add(new Npc { Name = "Vael", NpcType = NpcType.Mystic, Greeting = "Die Sterne flüstern die Lösung bereits." });

            var positions = new List<(int x, int y)> { (3, 10), (10, 10), (16, 6), (5, 4), (15, 14), (9, 4) };
            for (var i = 0; i < npcDefinitions.Count && i < positions.Count; i++)
            {
                var npc = npcDefinitions[i];
                var pos = positions[i];
                npc.PositionX = pos.x;
                npc.PositionY = pos.y;
                GameState.Npcs.Add(npc);
                var tile = GameState.Map.GetTile(pos.x, pos.y);
                if (tile != null && tile.Enemy == null)
                    tile.Npc = npc;
            }
        }

        private List<(int x, int y)> GetValidSpawnPositions()
        {
            var positions = new List<(int x, int y)>();
            var map = GameState.Map;

            for (var y = 4; y < map.Height - 2; y++)
            for (var x = 4; x < map.Width - 2; x++)
            {
                if (Math.Abs(x - SpawnX) + Math.Abs(y - SpawnY) < 6) continue;
                var tile = map.GetTile(x, y);
                if (tile != null && tile.IsWalkable && !tile.HasPlayer && tile.Enemy == null
                    && tile.Npc == null && tile.TileType != TileType.Exit && tile.TileType != TileType.Puzzle)
                {
                    positions.Add((x, y));
                }
            }

            return positions.OrderBy(p => p.y).ThenBy(p => p.x).ToList();
        }

        private void InitializeDemoMap()
        {
            var map = GameState.Map;
            FillMap(map, GetFloorTileTypeForBiome(CurrentBiome));
            CreateBorderWalls(map);

            for (var x = 4; x <= 10; x++) SetTile(map, x, 6, TileType.Wall);
            for (var y = 1; y <= 8; y++) SetTile(map, 13, y, TileType.Wall);
            SetTile(map, 13, 6, TileType.Floor);
            for (var x = 1; x <= 7; x++) SetTile(map, x, 12, TileType.Wall);
            for (var x = 9; x <= 13; x++) SetTile(map, x, 12, TileType.Wall);
            for (var y = 12; y <= 18; y++) SetTile(map, 13, y, TileType.Wall);
            SetTile(map, 13, 15, TileType.Floor);

            if (GameState.CurrentFloor >= 2)
            {
                for (var y = 8; y <= 16; y++) SetTile(map, 8, y, TileType.Wall);
                ConfigureDoor(map, 8, 10, $"amber-{GameState.CurrentFloor}", "The amber seal hums with druidic magic.");
                SetTile(map, 8, 14, TileType.Floor);
            }

            if (GameState.CurrentFloor >= 3)
            {
                for (var x = 10; x <= 16; x++) SetTile(map, x, 9, TileType.Wall);
                ConfigureDoor(map, 12, 9, $"ivory-{GameState.CurrentFloor}", "Only an ivory key from the puzzle wing will open this path.");
                SetTile(map, 15, 9, TileType.Floor);
            }

            var healTile = map.GetTile(16, 4);
            if (healTile != null) healTile.TileType = GetHealingTileTypeForBiome(CurrentBiome);
            var trap1 = map.GetTile(5, 15);
            if (trap1 != null) trap1.TileType = GetTrapTileTypeForBiome(CurrentBiome);
            var trap2 = map.GetTile(9, 16);
            if (trap2 != null) trap2.TileType = GetTrapTileTypeForBiome(CurrentBiome);
            var trap3 = map.GetTile(15, 11);
            if (trap3 != null && GameState.CurrentFloor >= 2) trap3.TileType = GetTrapTileTypeForBiome(CurrentBiome);

            AddItem(10, 3, new Potion("Health Potion", 25 + (GameState.CurrentFloor - 1) * 5));
            AddItem(7, 9, new Item("Gold Pile", ItemType.Gold));
            AddItem(15, 5, new Potion("Greater Potion", 30 + GameState.CurrentFloor * 5));
            AddItem(4, 16, new Item("Gold Cache", ItemType.Gold));

            AddKey(11, 14, new KeyItem("Amber Key", $"amber-{GameState.CurrentFloor}"));
            if (GameState.CurrentFloor >= 3)
                AddKey(6, 16, new KeyItem("Ivory Key", $"ivory-{GameState.CurrentFloor}"));

            AddPuzzle(5, 5, new PuzzleEncounter
            {
                PuzzleId = $"riddle-{GameState.CurrentFloor}",
                Title = "Whispering Runes",
                Riddle = "I open mouths of stone, yet weigh less than air. What am I?",
                Hint = "The chronicler speaks often of the right word.",
                RewardText = "The runes accept 'key' as the answer and reveal hidden treasure."
            });

            var pedestal = map.GetTile(14, 4);
            if (pedestal != null)
            {
                pedestal.TileType = TileType.KeyPedestal;
                pedestal.HintText = "Pedestal inscription: 'Steel opens war, words open worlds.'";
            }

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

        private void ApplyPlayerClass(PlayerClass selectedClass)
        {
            PlayerClassService.ApplyClass(GameState.Player, selectedClass);
            PlayerViewModel.UpdateStatus();
        }

        private void UpdateBiomeDisplay()
        {
            BiomeDisplayName = CurrentBiome switch
            {
                BiomeType.Forest => "Forest Depths",
                BiomeType.Crypt => "Haunted Crypt",
                BiomeType.Volcanic => "Volcanic Core",
                BiomeType.Underwater => "Sunken Ruins",
                BiomeType.Celestial => "Celestial Gate",
                _ => CurrentBiome.ToString()
            };
        }

        private void AddItem(int x, int y, Item item)
        {
            var tile = GameState.Map.GetTile(x, y);
            if (tile != null)
                tile.Item = item;
        }

        private void AddKey(int x, int y, KeyItem key)
        {
            var tile = GameState.Map.GetTile(x, y);
            if (tile != null)
            {
                tile.Item = key;
                tile.TileType = TileType.KeyPedestal;
                tile.HintText = $"A key rests here for lock '{key.KeyId}'.";
            }
        }

        private void AddPuzzle(int x, int y, PuzzleEncounter puzzle)
        {
            GameState.Puzzles.Add(puzzle);
            var tile = GameState.Map.GetTile(x, y);
            if (tile != null)
            {
                tile.TileType = TileType.Puzzle;
                tile.PuzzleId = puzzle.PuzzleId;
                tile.HintText = puzzle.Hint;
            }
        }

        private void ConfigureDoor(DungeonMap map, int x, int y, string keyId, string hint)
        {
            var tile = map.GetTile(x, y);
            if (tile != null)
            {
                tile.TileType = TileType.LockedDoor;
                tile.DoorKeyId = keyId;
                tile.HintText = hint;
            }
        }

        private static TileType GetFloorTileTypeForBiome(BiomeType biomeType) => biomeType switch
        {
            BiomeType.Forest => TileType.Floor,
            BiomeType.Crypt => TileType.CursedFloor,
            BiomeType.Volcanic => TileType.AshFloor,
            BiomeType.Underwater => TileType.SandFloor,
            BiomeType.Celestial => TileType.CloudFloor,
            _ => TileType.Floor
        };

        private static TileType GetTrapTileTypeForBiome(BiomeType biomeType) => biomeType switch
        {
            BiomeType.Forest => TileType.ThornTrap,
            BiomeType.Crypt => TileType.CurseTrap,
            BiomeType.Volcanic => TileType.LavaTrap,
            BiomeType.Underwater => TileType.SpikeTrap,
            BiomeType.Celestial => TileType.DivineTrap,
            _ => TileType.Trap
        };

        private static TileType GetHealingTileTypeForBiome(BiomeType biomeType) => biomeType switch
        {
            BiomeType.Forest => TileType.HealingShrine,
            BiomeType.Crypt => TileType.HealingAltar,
            BiomeType.Volcanic => TileType.HotSpring,
            BiomeType.Underwater => TileType.HealingBubble,
            BiomeType.Celestial => TileType.LightCircle,
            _ => TileType.HealingRoom
        };

        private static void FillMap(DungeonMap map, TileType tileType)
        {
            for (var y = 0; y < map.Height; y++)
            for (var x = 0; x < map.Width; x++)
                map.SetTile(x, y, new Tile(x, y, tileType));
        }

        private static void CreateBorderWalls(DungeonMap map)
        {
            for (var x = 0; x < map.Width; x++)
            {
                map.SetTile(x, 0, new Tile(x, 0, TileType.Wall));
                map.SetTile(x, map.Height - 1, new Tile(x, map.Height - 1, TileType.Wall));
            }
            for (var y = 0; y < map.Height; y++)
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
