namespace DungeonOfTheFallen.Core.Models
{
    public class GameState
    {
        public Player Player { get; set; }
        public DungeonMap Map { get; set; }
        public List<Enemy> Enemies { get; set; }
        public List<string> CombatLog { get; set; }
        public bool IsGameOver { get; set; }
        public bool IsVictory { get; set; }

        public GameState(int dungeonWidth = 20, int dungeonHeight = 20)
        {
            Player = new Player("Hero");
            Map = new DungeonMap(dungeonWidth, dungeonHeight);
            Enemies = new List<Enemy>();
            CombatLog = new List<string>();
            IsGameOver = false;
            IsVictory = false;
        }

        public void AddCombatLogEntry(string message)
        {
            CombatLog.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
