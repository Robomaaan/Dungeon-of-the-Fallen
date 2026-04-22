namespace DungeonOfTheFallen.Core.Models
{
    public enum TileType
    {
        Floor,          // begehbar
        Wall,           // nicht begehbar
        Exit,           // Ausgang zum Sieg
        Spawn,          // Startposition
        Trap,           // Falle - Schaden bei Betreten
        HealingRoom     // Heilraum - Regeneration
    }
}
