namespace DungeonOfTheFallen.Core.Models
{
    public class PuzzleEncounter
    {
        public string PuzzleId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Riddle { get; set; } = string.Empty;
        public string Hint { get; set; } = string.Empty;
        public string RewardText { get; set; } = string.Empty;
        public bool Solved { get; set; }
    }
}
