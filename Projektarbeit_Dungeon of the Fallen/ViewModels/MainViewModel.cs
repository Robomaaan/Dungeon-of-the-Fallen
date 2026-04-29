using System.Collections.ObjectModel;
using System.Windows.Input;
using DungeonOfTheFallen.Core.Models;
using DungeonOfTheFallen.Core.Persistence;
using DungeonOfTheFallen.Core.Services;
using Projektarbeit_Dungeon_of_the_Fallen.Services;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // ── Constants ────────────────────────────────────────────────────────
        private const int DungeonSize        = 20;
        private const int TileSize           = 64;
        private const int TileHeight         = TileSize * 3 / 4;
        private const int RenderMarginX      = 40;
        private const int RenderMarginY      = 48;
        private const int CombatLogMaxEntries = 12;
        private const int SpawnX           = 3;
        private const int SpawnY           = 3;
        private const int ExitX            = 17;
        private const int ExitY            = 17;
        private const string SaveFileName  = "dungeon_save.xml";

        // Dungeon layout: 4 rooms of 8×8 tiles connected by 2-tile-wide corridors.
        //
        //   Room 0 (Spawn)  x=1..8  y=1..8    ──corridor──  Room 1 (East)  x=11..18  y=1..8
        //        │                                                  │
        //   corridor                                           corridor
        //        │                                                  │
        //   Room 2 (South)  x=1..8  y=11..18  ──corridor──  Room 3 (Boss)  x=11..18  y=11..18
        //
        // Corridor positions:
        //   Spawn ↔ East  : x=9..10, y=4..5
        //   Spawn ↔ South : x=4..5, y=9..10
        //   East  ↔ Boss  : x=14..15, y=9..10  (locked door at room-3 entrance)
        //   South ↔ Boss  : x=9..10, y=14..15

        // ── Fields ───────────────────────────────────────────────────────────
        private TurnManager _turnManager;
        private readonly XmlGameRepository _repository = new();
        private readonly AssetRegistry _assetRegistry  = new();
        private readonly List<Room> _rooms             = new();
        private Room _currentRoom                      = null!;

        private string _combatLog       = string.Empty;
        private string _biomeDisplayName = string.Empty;
        private int _renderSurfaceWidth  = 1;
        private int _renderSurfaceHeight = 1;
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
        private ICommand? _continueCommand;
        private ICommand? _saveAndReturnToMenuCommand;
        private ICommand? _returnToMenuCommand;
        private ICommand? _abandonRunCommand;

        // ── Properties ───────────────────────────────────────────────────────
        public GameState GameState { get; private set; }
        public PlayerViewModel PlayerViewModel { get; private set; }
        public ObservableCollection<TileViewModel> Tiles { get; } = new();
        public ObservableCollection<RenderObjectViewModel> RenderObjects { get; } = new();
        public BiomeType CurrentBiome { get; private set; }
        public int CurrentFloor => GameState.CurrentFloor;
        public GameFlowPhase GamePhase => GameState.CurrentPhase;
        public int RenderSurfaceWidth
        {
            get => _renderSurfaceWidth;
            private set => SetProperty(ref _renderSurfaceWidth, value);
        }

        public int RenderSurfaceHeight
        {
            get => _renderSurfaceHeight;
            private set => SetProperty(ref _renderSurfaceHeight, value);
        }

        public string FloorDisplay => $"Ebene {GameState.CurrentFloor}/{GameState.FinalFloor}";
        public string ObjectiveText => GameState.ExitUnlocked
            ? GamePhase == GameFlowPhase.LevelComplete
                ? "Ebene abgeschlossen. Drücke Weiter, um zur nächsten Ebene abzusteigen."
                : "Ziel erreicht: Gehe zum Ausgang."
            : GamePhase == GameFlowPhase.PostCombat
                ? "Sichere Zwischenphase: Du kannst heilen oder Weiter drücken."
                : $"Ziel: Besiege alle Gegner und den Boss ({GameState.EnemiesDefeatedOnFloor}/{GameState.FloorObjectiveTarget}).";
        public string ExitStatusText => GameState.ExitUnlocked
            ? "Ausgang frei"
            : $"Ausgang versiegelt · Schlüssel {GameState.CollectedKeyIds.Count} · Gegner übrig {GameState.RemainingFloorEnemies}";
        public string DungeonSystemsText =>
            $"Schlüssel: {string.Join(", ", GameState.CollectedKeyIds.DefaultIfEmpty("keine"))} · Rätsel gelöst: {GameState.Puzzles.Count(p => p.Solved)}/{GameState.Puzzles.Count}";
        public string RunStatusText => IsVictory
            ? "Alle Ebenen bezwungen."
            : IsGameOver
                ? "Die Expedition ist gescheitert."
                : GamePhase switch
                {
                    GameFlowPhase.PostCombat => $"{BiomeDisplayName} · sichere Zwischenphase · Level {GameState.Player.Level}",
                    GameFlowPhase.LevelComplete => $"{BiomeDisplayName} · Ebenenwechsel bereit · Level {GameState.Player.Level}",
                    GameFlowPhase.CombatStart => $"{BiomeDisplayName} · Kampfstart",
                    _ => $"{BiomeDisplayName} · Level {GameState.Player.Level} · {GameState.Player.Weapon.Summary}"
                };
        public string GamePhaseText => GamePhase switch
        {
            GameFlowPhase.Exploration  => "Erkundung",
            GameFlowPhase.CombatStart   => "Kampfbeginn",
            GameFlowPhase.PlayerTurn    => "Spielerzug",
            GameFlowPhase.EnemyTurn     => "Gegnerzug",
            GameFlowPhase.PostCombat    => "Sichere Zwischenphase",
            GameFlowPhase.LevelComplete => "Ebenenwechsel",
            GameFlowPhase.GameOver      => "Game Over",
            _                          => GamePhase.ToString()
        };
        // Sichere Zwischenphasen blockieren keine freie Bewegung, nur Gegnerzüge.
        public bool CanMove => GamePhase is GameFlowPhase.Exploration
            or GameFlowPhase.PostCombat
            or GameFlowPhase.LevelComplete;
        public bool CanContinue => GamePhase is GameFlowPhase.PostCombat or GameFlowPhase.LevelComplete;
        public string ContinueButtonText => GamePhase == GameFlowPhase.LevelComplete ? "Nächste Ebene" : "Weiter";

        public int DungeonWidth  => GameState.Map.Width;
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

        // ── Commands ─────────────────────────────────────────────────────────
        public ICommand MoveUpCommand    => _moveUpCommand    ??= new RelayCommand(_ => MovePlayer(0, -1), _ => CanMove);
        public ICommand MoveDownCommand  => _moveDownCommand  ??= new RelayCommand(_ => MovePlayer(0,  1), _ => CanMove);
        public ICommand MoveLeftCommand  => _moveLeftCommand  ??= new RelayCommand(_ => MovePlayer(-1, 0), _ => CanMove);
        public ICommand MoveRightCommand => _moveRightCommand ??= new RelayCommand(_ => MovePlayer( 1, 0), _ => CanMove);
        public ICommand RestartGameCommand          => _restartGameCommand          ??= new RelayCommand(_ => RestartGame());
        public ICommand UsePotionCommand            => _usePotionCommand            ??= new RelayCommand(_ => UsePotion(), _ => !IsGameOver);
        public ICommand SaveGameCommand             => _saveGameCommand             ??= new RelayCommand(_ => SaveGame(),  _ => !IsGameOver);
        public ICommand LoadGameCommand             => _loadGameCommand             ??= new RelayCommand(_ => LoadGame());
        public ICommand ContinueCommand             => _continueCommand             ??= new RelayCommand(_ => ContinueRun(), _ => CanContinue);
        public ICommand SaveAndReturnToMenuCommand  => _saveAndReturnToMenuCommand  ??= new RelayCommand(_ => SaveAndReturnToMenu(), _ => !IsGameOver);
        public ICommand ReturnToMenuCommand         => _returnToMenuCommand         ??= new RelayCommand(_ => ReturnToMenu());
        public ICommand AbandonRunCommand           => _abandonRunCommand           ??= new RelayCommand(_ => RequestAbandonRun());

        public event Action<Enemy>? CombatRequested;
        // Gefeuert wenn der Spieler zurück ins Hauptmenü möchte (mit oder ohne Speichern)
        public event Action? ReturnToMainMenuRequested;
        // Gefeuert wenn der Spieler um Bestätigung für Run-Abbruch gebeten wird
        public event Action? AbandonRunRequested;

        // ── Constructor ──────────────────────────────────────────────────────
        public MainViewModel(PlayerClass selectedClass = PlayerClass.Warrior)
        {
            GameState      = new GameState(DungeonSize, DungeonSize);
            _turnManager   = new TurnManager(GameState);
            PlayerViewModel = new PlayerViewModel(GameState.Player);

            InitializeRooms();
            ApplyPlayerClass(selectedClass);
            BuildFloor(1, resetProgress: true);
            RefreshTileCollection();
            RebuildRenderObjects();
            UpdateCombatLog($"Spiel gestartet als {GameState.Player.PlayerClass}! {FloorDisplay} | {BiomeDisplayName} | Finde Schlüssel, löse Rätsel, bezwinge die Bosskammer.");
        }

        // ── Room System ──────────────────────────────────────────────────────

        private void InitializeRooms()
        {
            _rooms.Clear();
            _rooms.Add(new Room("spawn", "Entrance Hall",    1,  1,  8,  8));
            _rooms.Add(new Room("east",  "Eastern Chamber", 11,  1, 18,  8));
            _rooms.Add(new Room("south", "Southern Crypt",   1, 11,  8, 18));
            _rooms.Add(new Room("boss",  "Boss Chamber",    11, 11, 18, 18));
            _currentRoom = _rooms[0];
        }

        private void CheckRoomTransition(int x, int y)
        {
            var room = _rooms.FirstOrDefault(r => r.Contains(x, y));
            if (room != null && room.Id != _currentRoom.Id)
                _currentRoom = room;
        }

        // ── Movement ─────────────────────────────────────────────────────────

        private void MovePlayer(int dx, int dy)
        {
            if (IsGameOver || IsVictory || !CanMove) return;

            var newX = GameState.Player.PositionX + dx;
            var newY = GameState.Player.PositionY + dy;
            if (newX < 0 || newX >= DungeonSize || newY < 0 || newY >= DungeonSize) return;

            var targetTile = GameState.Map.GetTile(newX, newY);
            if (targetTile == null || (!targetTile.IsWalkable && targetTile.TileType != TileType.LockedDoor)) return;

            if (targetTile.TileType == TileType.Exit && !GameState.ExitUnlocked)
            {
                GameState.AddCombatLogEntry("[AUSGANG] Das Tor ist versiegelt. Besiege den Boss und räume die Ebene.");
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
                    CheckRoomTransition(newX, newY);
                    UpdateAllTiles();
                    UpdateCombatLog();
                    CheckGameConditions();
                }
                return;
            }

            if (_turnManager.MovePlayer(newX, newY))
            {
                CheckRoomTransition(newX, newY);

                // Gegner ist während der Feind-Züge neben den Spieler gerückt → Kampf öffnen
                if (!IsGameOver && _turnManager.PendingEncounterEnemy != null)
                {
                    OpenCombat(_turnManager.PendingEncounterEnemy);
                }

                UpdateAllTiles();
                UpdateCombatLog();
                CheckGameConditions();
            }
            else
            {
                UpdateCombatLog();
            }
        }

        private void OpenCombat(Enemy enemy)
        {
            SetGamePhase(GameFlowPhase.CombatStart, $"[Phase] Kampfbeginn gegen {enemy.Name}.");
            CombatRequested?.Invoke(enemy);
        }

        // ── Actions ──────────────────────────────────────────────────────────

        private void UsePotion()
        {
            if (IsGameOver || IsVictory) return;

            var potion = GameState.Player.Inventory.Items.OfType<Potion>().FirstOrDefault();
            if (potion == null)
            {
                GameState.AddCombatLogEntry("[Trank] Kein Trank verfügbar.");
                UpdateCombatLog();
                return;
            }

            if (GameState.Player.HP >= GameState.Player.MaxHP)
            {
                GameState.AddCombatLogEntry("[Trank] HP bereits voll. Kein Trank verbraucht.");
                UpdateCombatLog();
                return;
            }

            var before = GameState.Player.HP;
            var heal = Math.Min(potion.HealingAmount, GameState.Player.MaxHP - GameState.Player.HP);
            GameState.Player.HP += heal;
            GameState.Player.Inventory.Remove(potion);
            GameState.AddCombatLogEntry($"[Trank] Spieler heilt {heal} HP: {before} → {GameState.Player.HP}.");
            GameState.AddCombatLogEntry("[Trank] Kein Gegnerzug ausgelöst.");
            UpdateAllTiles();
            UpdateCombatLog();
        }

        private void SaveGame()
        {
            var data = SaveDataMapper.ToSaveData(GameState);
            _repository.Save(data, SaveFileName);
            GameState.AddCombatLogEntry($"[SPEICHERN] {FloorDisplay} in {BiomeDisplayName} gespeichert!");
            UpdateCombatLog();
        }

        private void LoadGame()
        {
            var data = _repository.Load(SaveFileName);
            if (data == null)
            {
                GameState.AddCombatLogEntry("[LADEN] Keine Spielstandsdatei gefunden!");
                UpdateCombatLog();
                return;
            }

            BuildFloor(data.CurrentFloor, resetProgress: false);
            IsGameOver = false;
            IsVictory  = false;
            SaveDataMapper.ApplyToGameState(GameState, data);
            SetGamePhase(GameFlowPhase.Exploration);
            CurrentBiome = GameState.CurrentBiome;
            UpdateBiomeDisplay();
            OnPropertyChanged(nameof(CurrentFloor), nameof(FloorDisplay));

            GameState.AddCombatLogEntry($"[LADEN] Spielstand geladen! Speicherstand v{data.SaveVersion} auf {FloorDisplay} wiederhergestellt.");
            UpdateAllTiles();
            UpdateCombatLog();
            CheckGameConditions();
        }

        private void SaveAndReturnToMenu()
        {
            if (!IsGameOver) SaveGame();
            ReturnToMainMenuRequested?.Invoke();
        }

        private void ReturnToMenu()
        {
            ReturnToMainMenuRequested?.Invoke();
        }

        private void RequestAbandonRun()
        {
            AbandonRunRequested?.Invoke();
        }

        public void CompleteCombat(Enemy enemy, bool enemyDefeated)
        {
            if (IsGameOver)
                return;

            UpdateAllTiles();

            if (enemyDefeated && GameState.Player.IsAlive)
            {
                IsGameOver = false;
                IsVictory = false;
                SetGamePhase(GameFlowPhase.PostCombat, $"[Phase] {enemy.Name} besiegt. Sichere Zwischenphase gestartet.");
                return;
            }

            IsVictory = false;
            IsGameOver = true;
            SetGamePhase(GameFlowPhase.GameOver, "[Phase] Kampf verloren.");
        }

        public void DebugFullHeal()
        {
            var before = GameState.Player.HP;
            GameState.Player.HP = GameState.Player.MaxHP;
            GameState.AddCombatLogEntry($"[Debug] Spieler vollständig geheilt. HP: {before} → {GameState.Player.HP}.");
            UpdateAllTiles();
            UpdateCombatLog();
        }

        public void DebugToggleGodMode()
        {
            GameState.IsGodMode = !GameState.IsGodMode;
            GameState.AddCombatLogEntry(GameState.IsGodMode
                ? "[Debug] Godmode aktiviert."
                : "[Debug] Godmode deaktiviert.");
            UpdateCombatLog();
        }

        public void DebugAdvanceToNextFloor()
        {
            if (GameState.CurrentFloor >= GameState.FinalFloor)
            {
                GameState.AddCombatLogEntry("[Debug] Bereits auf der letzten Ebene.");
                UpdateCombatLog();
                return;
            }

            GameState.AddCombatLogEntry($"[Debug] Wechsel zu Ebene {GameState.CurrentFloor + 1}.");
            UpdateCombatLog();
            IsGameOver = false;
            IsVictory = false;
            AdvanceToNextFloor();
        }

        private void SetGamePhase(GameFlowPhase phase, string? logMessage = null)
        {
            GameState.CurrentPhase = phase;
            OnPropertyChanged(
                nameof(GamePhase),
                nameof(GamePhaseText),
                nameof(ObjectiveText),
                nameof(RunStatusText),
                nameof(CanMove),
                nameof(CanContinue),
                nameof(ContinueButtonText));
            CommandManager.InvalidateRequerySuggested();

            if (logMessage != null)
                GameState.AddCombatLogEntry(logMessage);

            UpdateCombatLog();
        }

        // ── Game Flow ────────────────────────────────────────────────────────

        private void CheckGameConditions()
        {
            if (_turnManager.CheckExitReached())
            {
                if (!GameState.ExitUnlocked)
                {
                    GameState.AddCombatLogEntry("[AUSGANG] Der Weg nach unten bleibt versiegelt.");
                    UpdateCombatLog();
                    return;
                }

                if (GameState.CurrentFloor >= GameState.FinalFloor)
                {
                    IsVictory  = true;
                    IsGameOver = true;
                    SetGamePhase(GameFlowPhase.GameOver, "[SIEG] Du hast alle Kerkerebenen bezwungen!");
                }
                else
                {
                    SetGamePhase(GameFlowPhase.LevelComplete, "[Phase] Ebene abgeschlossen. Drücke Weiter, um zur nächsten Ebene abzusteigen.");
                }
                UpdateCombatLog();
                return;
            }

            if (!GameState.Player.IsAlive)
            {
                IsGameOver = true;
                SetGamePhase(GameFlowPhase.GameOver, "[Phase] Die Expedition ist gescheitert.");
            }
        }

        private void AdvanceToNextFloor()
        {
            var nextFloor = GameState.CurrentFloor + 1;
            var heal = GameBalance.CalculateFloorTransitionHeal(GameState.Player, GameState.CurrentFloor);
            if (heal > 0)
                GameState.Player.HP += heal;

            GameState.Player.Inventory.Add(new Potion("Reisetrunk", 20 + nextFloor * 4));
            BuildFloor(nextFloor, resetProgress: false);
            RefreshTileCollection();
            UpdateAllTiles();
            SetGamePhase(GameFlowPhase.Exploration, $"[ETAGE] Hinabgestiegen zu {FloorDisplay}. Neues Biotop: {BiomeDisplayName}. Rastpause stellt {heal} HP wieder her.");
        }

        private void ContinueRun()
        {
            if (GamePhase == GameFlowPhase.PostCombat)
            {
                SetGamePhase(GameFlowPhase.Exploration, "[Phase] Zwischenphase beendet. Du kannst weiter erkunden.");
                return;
            }

            if (GamePhase == GameFlowPhase.LevelComplete)
            {
                AdvanceToNextFloor();
                return;
            }
        }

        private void RestartGame()
        {
            var playerClass = GameState.Player.PlayerClass;
            GameState       = new GameState(DungeonSize, DungeonSize);
            _turnManager    = new TurnManager(GameState);
            PlayerViewModel = new PlayerViewModel(GameState.Player);
            OnPropertyChanged(nameof(GameState), nameof(PlayerViewModel), nameof(DungeonWidth), nameof(DungeonHeight));

            ApplyPlayerClass(playerClass);
            BuildFloor(1, resetProgress: true);
            IsGameOver = false;
            IsVictory  = false;

            RefreshTileCollection();
            UpdateAllTiles();
            SetGamePhase(GameFlowPhase.Exploration);
            UpdateCombatLog($"Spiel neu gestartet als {GameState.Player.PlayerClass} auf {FloorDisplay} in {BiomeDisplayName}.");
        }

        // ── Floor Building ───────────────────────────────────────────────────

        private void BuildFloor(int floor, bool resetProgress)
        {
            if (resetProgress)
            {
                GameState.Player.XP    = 0;
                GameState.Player.Level = 1;
                GameState.Player.Gold  = 0;
                GameState.CollectedKeyIds.Clear();
            }

            GameState.CurrentFloor  = floor;
            GameState.CurrentBiome  = FloorThemeService.GetBiomeForFloor(floor);
            GameState.ExitUnlocked  = false;
            GameState.BossDefeatedOnFloor   = false;
            GameState.EnemiesDefeatedOnFloor = 0;
            GameState.CurrentPhase = GameFlowPhase.Exploration;
            CurrentBiome = GameState.CurrentBiome;
            UpdateBiomeDisplay();

            // Reset current room to spawn
            _currentRoom = _rooms.Count > 0 ? _rooms.First(r => r.Id == "spawn") : _rooms.FirstOrDefault()!;

            GameState.Enemies.Clear();
            GameState.Npcs.Clear();
            GameState.Puzzles.Clear();
            foreach (var tile in GameState.Map.GetAllTiles())
            {
                tile.HasPlayer = false;
                tile.Enemy     = null;
                tile.Item      = null;
                tile.Npc       = null;
                tile.DoorKeyId  = null;
                tile.PuzzleId   = null;
                tile.HintText   = null;
            }

            InitializeDemoMap();
            SpawnEnemies();
            SpawnNpcs();
        }

        // ── Map Layout ───────────────────────────────────────────────────────
        // Layout (20×20):
        //  [0]  = border wall
        //  [R0] = Spawn room   x=1..8,  y=1..8
        //  [R1] = East room    x=11..18, y=1..8
        //  [R2] = South room   x=1..8,  y=11..18
        //  [R3] = Boss room    x=11..18, y=11..18
        //  Corridors (floor): see diagram in class header

        private void InitializeDemoMap()
        {
            var map      = GameState.Map;
            var floor    = GetFloorTileTypeForBiome(CurrentBiome);
            var healType = GetHealingTileTypeForBiome(CurrentBiome);
            var trapType = GetTrapTileTypeForBiome(CurrentBiome);

            // 1. Fill everything with walls
            for (var y = 0; y < map.Height; y++)
            for (var x = 0; x < map.Width;  x++)
                map.SetTile(x, y, new Tile(x, y, TileType.Wall));

            // 2. Carve rooms
            foreach (var room in _rooms)
                CarveFloor(map, room.X1, room.Y1, room.X2, room.Y2, floor);

            // 3. Carve corridors
            CarveFloor(map,  9,  4, 10,  5, floor);   // Spawn ↔ East
            CarveFloor(map,  4,  9,  5, 10, floor);   // Spawn ↔ South
            CarveFloor(map, 14,  9, 15, 10, floor);   // East  ↔ Boss
            CarveFloor(map,  9, 14, 10, 15, floor);   // South ↔ Boss

            // 4. Locked door: entrance to Boss room from East corridor (amber key)
            SetTileType(map, 14, 11, TileType.LockedDoor);
            var d1 = map.GetTile(14, 11);
            if (d1 != null) { d1.DoorKeyId = $"amber-{GameState.CurrentFloor}"; d1.HintText = "Die Tore zur Bosskammer sind versiegelt. Du benötigst den Bernsteinschlüssel."; }

            // Floor 3+: second locked door from South corridor (ivory key)
            if (GameState.CurrentFloor >= 3)
            {
                SetTileType(map, 11, 14, TileType.LockedDoor);
                var d2 = map.GetTile(11, 14);
                if (d2 != null) { d2.DoorKeyId = $"ivory-{GameState.CurrentFloor}"; d2.HintText = "Nur ein Elfenbeinschlüssel aus der südlichen Gruft öffnet dieses Tor."; }
            }

            // 5. Spawn & Exit
            SetTileType(map, SpawnX, SpawnY, TileType.Spawn);
            var spawnTile = map.GetTile(SpawnX, SpawnY);
            if (spawnTile != null) spawnTile.HasPlayer = true;
            GameState.Player.PositionX = SpawnX;
            GameState.Player.PositionY = SpawnY;

            SetTileType(map, ExitX, ExitY, TileType.Exit);

            // 6. Healing shrines (one per room)
            SetTileType(map, 7, 2, healType);
            SetTileType(map, 17, 2, healType);
            SetTileType(map, 7, 17, healType);
            SetTileType(map, 12, 12, healType);

            // 7. Traps
            SetTileType(map, 13, 5, trapType);   // East room
            SetTileType(map,  4, 14, trapType);  // South room
            if (GameState.CurrentFloor >= 2)
            {
                SetTileType(map, 16, 6, trapType);   // East room extra
                SetTileType(map,  3, 15, trapType);  // South room extra
            }

            // 8. Items
            AddItem(6, 6, new Potion("Heiltrank",     25 + (GameState.CurrentFloor - 1) * 5));
            AddItem(16, 3, new Item("Goldhaufen",     ItemType.Gold));
            AddItem(15, 15, new Potion("Großer Heiltrank", 30 + GameState.CurrentFloor * 5));
            AddItem(3, 17, new Item("Goldversteck",   ItemType.Gold));

            // 9. Keys
            AddKey(17, 6, new KeyItem("Bernsteinschlüssel", $"amber-{GameState.CurrentFloor}"));
            if (GameState.CurrentFloor >= 3)
                AddKey(7, 16, new KeyItem("Elfenbeinschlüssel", $"ivory-{GameState.CurrentFloor}"));

            // 10. Puzzle (South room)
            AddPuzzle(3, 16, new PuzzleEncounter
            {
                PuzzleId   = $"riddle-{GameState.CurrentFloor}",
                Title      = "Flüsternde Runen",
                Riddle     = "Ich öffne Münder aus Stein, doch wiege weniger als Luft. Was bin ich?",
                Hint       = "Der Chronist spricht oft vom richtigen Wort.",
                RewardText = "Die Runen akzeptieren 'Schlüssel' als Antwort und enthüllen verborgene Schätze."
            });

            // 11. Key pedestal (Boss room)
            SetTileType(map, 15, 16, TileType.KeyPedestal);
            var pedestal = map.GetTile(15, 16);
            if (pedestal != null) pedestal.HintText = "Inschrift am Altar: 'Stahl öffnet Kriege, Worte öffnen Welten.'";
        }

        // ── Spawning ─────────────────────────────────────────────────────────

        private void SpawnEnemies()
        {
            var positions = GetValidSpawnPositions();
            var roster    = EnemySpawnService.CreateRoster(CurrentBiome, GameState.CurrentFloor).ToList();
            GameState.FloorObjectiveTarget = roster.Count;

            foreach (var enemy in roster.Where(e => !e.IsBoss))
            {
                if (positions.Count == 0) break;
                PlaceEnemy(enemy, positions[0].x, positions[0].y);
                positions.RemoveAt(0);
            }

            // Boss always placed in the Boss room at a random free position
            var boss     = roster.FirstOrDefault(e => e.IsBoss);
            var bossRoom = _rooms.Find(r => r.Id == "boss");
            if (boss != null && bossRoom != null)
            {
                var bossPos = GetRandomRoomPosition(bossRoom);
                PlaceEnemy(boss, bossPos.x, bossPos.y);
            }
        }

        private void SpawnNpcs()
        {
            var npcDefs = new[]
            {
                new Npc { Name = "Sister Mirel",    NpcType = NpcType.Healer,     Greeting = "Ruhe deinen Geist aus." },
                new Npc { Name = "Dorin",           NpcType = NpcType.Merchant,   Greeting = "Vorräte gegen Gold oder Mut gegen Armut." },
                new Npc { Name = "Archivist Oren",  NpcType = NpcType.Chronicler, Greeting = "Hinter jedem Siegel wartet eine Geschichte." },
                new Npc { Name = "Brakka",          NpcType = NpcType.Blacksmith, Greeting = "Zeig mir deine Klinge." },
                new Npc { Name = "Lysa",            NpcType = NpcType.Scout,      Greeting = "Ich beobachte jede Bewegung hier unten." }
            }.ToList();

            if (GameState.CurrentFloor >= 3)
                npcDefs.Add(new Npc { Name = "Vael", NpcType = NpcType.Mystic, Greeting = "Die Sterne flüstern die Lösung bereits." });

            // Positions within the new room layout
            var positions = new List<(int x, int y)>
            {
                (7, 7),    // Room 0: Healer
                (17, 7),   // Room 1: Merchant
                (2, 12),   // Room 2: Chronicler
                (2, 2),    // Room 0: Blacksmith
                (17, 12),  // Room 3: Scout
                (12, 3),   // Room 1: Mystic (floor 3+)
            };

            for (var i = 0; i < npcDefs.Count && i < positions.Count; i++)
            {
                var npc = npcDefs[i];
                var pos = positions[i];
                npc.PositionX = pos.x;
                npc.PositionY = pos.y;
                GameState.Npcs.Add(npc);
                var tile = GameState.Map.GetTile(pos.x, pos.y);
                if (tile != null && tile.Enemy == null)
                    tile.Npc = npc;
            }
        }

        // Returns shuffled valid spawn positions across all non-spawn rooms.
        private List<(int x, int y)> GetValidSpawnPositions()
        {
            var map       = GameState.Map;
            var positions = new List<(int x, int y)>();

            foreach (var room in _rooms.Where(r => r.Id != "spawn"))
            {
                for (var y = room.Y1; y <= room.Y2; y++)
                for (var x = room.X1; x <= room.X2; x++)
                {
                    var tile = map.GetTile(x, y);
                    if (tile != null && tile.IsWalkable && !tile.HasPlayer
                        && tile.Enemy == null && tile.Npc == null
                        && tile.TileType != TileType.Exit
                        && tile.TileType != TileType.Puzzle
                        && tile.TileType != TileType.LockedDoor)
                        positions.Add((x, y));
                }
            }

            // Fisher-Yates shuffle for truly random placement
            for (var i = positions.Count - 1; i > 0; i--)
            {
                var j = Random.Shared.Next(i + 1);
                (positions[i], positions[j]) = (positions[j], positions[i]);
            }

            return positions;
        }

        private (int x, int y) GetRandomRoomPosition(Room room)
        {
            var map        = GameState.Map;
            var candidates = new List<(int x, int y)>();
            for (var y = room.Y1; y <= room.Y2; y++)
            for (var x = room.X1; x <= room.X2; x++)
            {
                var tile = map.GetTile(x, y);
                if (tile?.IsWalkable == true && tile.Enemy == null && tile.Npc == null
                    && tile.TileType != TileType.Exit && tile.TileType != TileType.Spawn)
                    candidates.Add((x, y));
            }
            return candidates.Count > 0
                ? candidates[Random.Shared.Next(candidates.Count)]
                : (room.CenterX, room.CenterY);
        }

        private void PlaceEnemy(Enemy enemy, int x, int y)
        {
            enemy.PositionX = x;
            enemy.PositionY = y;
            GameState.Enemies.Add(enemy);
            var tile = GameState.Map.GetTile(x, y);
            if (tile != null) tile.Enemy = enemy;
        }

        // ── Tile & Player helpers ────────────────────────────────────────────

        public void RefreshTileCollection()
        {
            Tiles.Clear();
            for (var y = 0; y < GameState.Map.Height; y++)
            for (var x = 0; x < GameState.Map.Width;  x++)
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
            RebuildRenderObjects();
            OnPropertyChanged(nameof(CurrentFloor), nameof(FloorDisplay), nameof(ObjectiveText),
                nameof(ExitStatusText), nameof(RunStatusText), nameof(DungeonSystemsText));
        }

        private void UpdateCombatLog(string? overrideMessage = null)
        {
            if (overrideMessage != null) { CombatLog = overrideMessage; return; }
            if (GameState.CombatLog.Count > 0)
                CombatLog = string.Join("\n", GameState.CombatLog.TakeLast(CombatLogMaxEntries));
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
                BiomeType.Forest     => "Waldtiefen",
                BiomeType.Crypt      => "Verfluchte Gruft",
                BiomeType.Volcanic   => "Vulkankern",
                BiomeType.Underwater => "Versunkene Ruinen",
                BiomeType.Celestial  => "Himmelstor",
                _                    => CurrentBiome.ToString()
            };
        }

        private void AddItem(int x, int y, Item item)
        {
            var tile = GameState.Map.GetTile(x, y);
            if (tile != null) tile.Item = item;
        }

        private void AddKey(int x, int y, KeyItem key)
        {
            var tile = GameState.Map.GetTile(x, y);
            if (tile != null) { tile.Item = key; tile.TileType = TileType.KeyPedestal; tile.HintText = $"Hier liegt ein Schlüssel für Schloss '{key.KeyId}'."; }
        }

        private void AddPuzzle(int x, int y, PuzzleEncounter puzzle)
        {
            GameState.Puzzles.Add(puzzle);
            var tile = GameState.Map.GetTile(x, y);
            if (tile != null) { tile.TileType = TileType.Puzzle; tile.PuzzleId = puzzle.PuzzleId; tile.HintText = puzzle.Hint; }
        }

        // ── Map building helpers ─────────────────────────────────────────────

        private static void CarveFloor(DungeonMap map, int x1, int y1, int x2, int y2, TileType floor)
        {
            for (var y = y1; y <= y2; y++)
            for (var x = x1; x <= x2; x++)
                map.SetTile(x, y, new Tile(x, y, floor));
        }

        private static void SetTileType(DungeonMap map, int x, int y, TileType type)
        {
            var tile = map.GetTile(x, y);
            if (tile != null) tile.TileType = type;
        }

        private static TileType GetFloorTileTypeForBiome(BiomeType b) => b switch
        {
            BiomeType.Forest     => TileType.Floor,
            BiomeType.Crypt      => TileType.CursedFloor,
            BiomeType.Volcanic   => TileType.AshFloor,
            BiomeType.Underwater => TileType.SandFloor,
            BiomeType.Celestial  => TileType.CloudFloor,
            _                    => TileType.Floor
        };

        private static TileType GetTrapTileTypeForBiome(BiomeType b) => b switch
        {
            BiomeType.Forest     => TileType.ThornTrap,
            BiomeType.Crypt      => TileType.CurseTrap,
            BiomeType.Volcanic   => TileType.LavaTrap,
            BiomeType.Underwater => TileType.SpikeTrap,
            BiomeType.Celestial  => TileType.DivineTrap,
            _                    => TileType.Trap
        };

        private static TileType GetHealingTileTypeForBiome(BiomeType b) => b switch
        {
            BiomeType.Forest     => TileType.HealingShrine,
            BiomeType.Crypt      => TileType.HealingAltar,
            BiomeType.Volcanic   => TileType.HotSpring,
            BiomeType.Underwater => TileType.HealingBubble,
            BiomeType.Celestial  => TileType.LightCircle,
            _                    => TileType.HealingRoom
        };

        // ── Rendering ────────────────────────────────────────────────────────

        // Renders only the current room + 2-tile border (to show corridor exits).
        // Tile positions are snapped to the native 64x48 sprite grid so the
        // canvas stays on whole pixels and the floor cells do not show seams.
        private void RebuildRenderObjects()
        {
            const int OffsetX = RenderMarginX;
            const int OffsetY = RenderMarginY;

            RenderObjects.Clear();
            var map  = GameState.Map;
            var room = _currentRoom;
            if (room == null) return;

            // Extend by 2 tiles in each direction to show corridor openings
            var vx1 = Math.Max(0, room.X1 - 2);
            var vy1 = Math.Max(0, room.Y1 - 2);
            var vx2 = Math.Min(map.Width  - 1, room.X2 + 2);
            var vy2 = Math.Min(map.Height - 1, room.Y2 + 2);
            var wallLift = (int)Math.Floor(TileHeight * 0.58);
            var wallHeight = (int)Math.Floor(TileHeight * 1.62);
            var charH = (int)Math.Floor(TileHeight * 1.8);
            var itemSize = (int)Math.Floor(Math.Min(TileSize, TileHeight) * 0.66);
            var entityWidth = TileSize + 2;
            var visibleWidth = vx2 - vx1 + 1;
            var visibleHeight = vy2 - vy1 + 1;

            RenderSurfaceWidth = OffsetX * 2 + (visibleWidth * TileSize) + 8;
            RenderSurfaceHeight = OffsetY * 2 + (visibleHeight * TileHeight) + 8;

            for (var y = vy1; y <= vy2; y++)
            for (var x = vx1; x <= vx2; x++)
            {
                var tile = map.GetTile(x, y);
                if (tile == null) continue;

                var sx = OffsetX + (x - vx1) * TileSize;
                var sy = OffsetY + (y - vy1) * TileHeight;
                var bz = y * 100;

                // Floor (always rendered as base)
                RenderObjects.Add(new RenderObjectViewModel(
                    _assetRegistry.GetFloorAsset(tile),
                    sx, sy,
                    TileSize, TileHeight,
                    bz, $"F_{x}_{y}", "floor"));

                // Wall (taller, rendered above floor)
                if (tile.TileType == TileType.Wall)
                {
                    RenderObjects.Add(new RenderObjectViewModel(
                        _assetRegistry.GetWallAsset(tile, map),
                        sx, sy - wallLift,
                        TileSize, wallHeight,
                        bz + 40, $"W_{x}_{y}", "wall"));
                    continue;
                }

                // Special tile overlay — only rendered for tiles that actually have one
                if (HasSpecialLayer(tile.TileType))
                {
                    RenderObjects.Add(new RenderObjectViewModel(
                        _assetRegistry.GetSpecialTileAsset(tile),
                        sx, sy,
                        TileSize, TileHeight,
                        bz + 10, $"S_{tile.TileType}_{x}_{y}", "special"));
                }

                // Item
                if (tile.Item != null)
                {
                    RenderObjects.Add(new RenderObjectViewModel(
                        _assetRegistry.GetItemAsset(tile),
                        sx + (TileSize - itemSize) / 2, sy + (TileHeight - itemSize) / 2,
                        itemSize, itemSize, bz + 20, $"I_{x}_{y}", "item"));
                }

                // Enemy
                if (tile.Enemy?.IsAlive == true)
                    RenderObjects.Add(new RenderObjectViewModel(
                        _assetRegistry.GetEnemyAsset(tile.Enemy),
                        sx - 1, sy - charH + TileHeight,
                        entityWidth, charH, bz + 50, $"E_{x}_{y}", "entity"));

                // NPC
                if (tile.Npc != null)
                    RenderObjects.Add(new RenderObjectViewModel(
                        _assetRegistry.GetNpcAsset(tile.Npc),
                        sx - 1, sy - charH + TileHeight,
                        entityWidth, charH, bz + 50, $"N_{x}_{y}", "entity"));

                // Player
                if (tile.HasPlayer)
                    RenderObjects.Add(new RenderObjectViewModel(
                        _assetRegistry.GetPlayerAsset(GameState.Player),
                        sx - 1, sy - charH + TileHeight,
                        entityWidth, charH, bz + 60, $"P_{x}_{y}", "player"));
            }
        }

        // Returns true only for tile types that have a dedicated special-layer sprite.
        // Regular floors, walls and unrecognized types must NOT get a special overlay.
        private static bool HasSpecialLayer(TileType t) => t is
            TileType.LockedDoor   or TileType.Exit         or
            TileType.ThornTrap    or TileType.CurseTrap    or TileType.LavaTrap  or
            TileType.SpikeTrap    or TileType.DivineTrap   or
            TileType.HealingRoom   or
            TileType.HealingShrine or TileType.HealingAltar or TileType.HotSpring or
            TileType.HealingBubble or
            TileType.Puzzle       or TileType.KeyPedestal;

    }
}
