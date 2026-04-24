namespace DungeonOfTheFallen.Core.Models
{
    public class CombatantStats
    {
        public int MaxHP { get; set; }
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }

        public bool IsAlive => HP > 0;

        public void RestoreToFullHealth()
        {
            HP = MaxHP;
        }
    }
}
