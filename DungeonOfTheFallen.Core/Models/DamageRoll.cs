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

        public string Describe()
        {
            var bonusText = Bonus >= 0 ? $"+{Bonus}" : Bonus.ToString();
            return $"{Count}W{(int)DieSize}{bonusText} {GetGermanDamageType()}";
        }

        private string GetGermanDamageType() => DamageType switch
        {
            DamageType.Physical   => "physischer Schaden",
            DamageType.Slashing   => "Hiebschaden",
            DamageType.Piercing   => "Stichschaden",
            DamageType.Bludgeoning => "Wuchtschaden",
            DamageType.Fire      => "Feuerschaden",
            DamageType.Cold      => "Kälteschaden",
            DamageType.Lightning => "Blitzschaden",
            DamageType.Poison    => "Giftschaden",
            DamageType.Necrotic   => "Nekrotischer Schaden",
            DamageType.Radiant   => "Strahlenschaden",
            DamageType.Arcane    => "Arkanschaden",
            _                    => DamageType.ToString()
        };
    }
}
