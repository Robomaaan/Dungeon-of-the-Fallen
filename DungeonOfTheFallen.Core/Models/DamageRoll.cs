namespace DungeonOfTheFallen.Core.Models
{
    public class DamageRoll
    {
        public int Count { get; set; }
        public DieSize DieSize { get; set; }
        public int Bonus { get; set; }
        public DamageType DamageType { get; set; } = DamageType.Physical;
        public string Label { get; set; } = string.Empty;

        public int Roll(bool criticalHit = false)
        {
            var count = criticalHit ? Count * 2 : Count;
            return Dice.Roll(count, (int)DieSize) + Bonus;
        }

        public string Describe() => $"{Count}d{(int)DieSize}{(Bonus >= 0 ? $"+{Bonus}" : Bonus)} {DamageType}";
    }
}
