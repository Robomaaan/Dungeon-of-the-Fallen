namespace DungeonOfTheFallen.Core.Models
{
    public class GameState
    {
        public Player Player { get; }
        public DungeonMap Map { get; }
        public List<Enemy> Enemies { get; } = new();
        public List<Npc> Npcs { get; } = new();
        public List<string> CombatLog { get; } = new();
        public List<string> CollectedKeyIds { get; } = new();
        public List<PuzzleEncounter> Puzzles { get; } = new();
        public bool IsGameOver { get; set; }
        public bool IsVictory { get; set; }
        public int CurrentFloor { get; set; } = 1;
        public int FinalFloor { get; set; } = 4;
        public BiomeType CurrentBiome { get; set; } = BiomeType.Forest;
        public GameFlowPhase CurrentPhase { get; set; } = GameFlowPhase.Exploration;
        public bool IsGodMode { get; set; }
        public int FloorObjectiveTarget { get; set; }
        public int EnemiesDefeatedOnFloor { get; set; }
        public bool BossDefeatedOnFloor { get; set; }
        public bool ExitUnlocked { get; set; }

        public int RemainingFloorEnemies => Math.Max(0, FloorObjectiveTarget - EnemiesDefeatedOnFloor);

        public GameState(int dungeonWidth = 20, int dungeonHeight = 20)
        {
            Player = new Player("Held");
            Map = new DungeonMap(dungeonWidth, dungeonHeight);
        }

        public bool HasKey(string keyId) => CollectedKeyIds.Contains(keyId);

        public void AddKey(string keyId)
        {
            if (!CollectedKeyIds.Contains(keyId))
                CollectedKeyIds.Add(keyId);
        }

        public void AddCombatLogEntry(string message)
        {
            CombatLog.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
