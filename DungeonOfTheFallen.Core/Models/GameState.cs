namespace DungeonOfTheFallen.Core.Models
{
    public class GameState
    {
        public Player Player { get; }
        public DungeonMap Map { get; }
        public List<Enemy> Enemies { get; } = new();
        public List<string> CombatLog { get; } = new();
        public bool IsGameOver { get; set; }
        public bool IsVictory { get; set; }

        public GameState(int dungeonWidth = 20, int dungeonHeight = 20)
        {
            Player = new Player("Hero");
            Map = new DungeonMap(dungeonWidth, dungeonHeight);
        }

        public void AddCombatLogEntry(string message)
        {
            CombatLog.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
