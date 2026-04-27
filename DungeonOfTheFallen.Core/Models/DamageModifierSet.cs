namespace DungeonOfTheFallen.Core.Models
{
    public class DamageModifierSet
    {
        public List<DamageType> Resistances { get; set; } = new();
        public List<DamageType> Vulnerabilities { get; set; } = new();
        public List<DamageType> Immunities { get; set; } = new();

        public int Apply(DamageType damageType, int amount)
        {
            if (amount <= 0)
                return 0;

            if (Immunities.Contains(damageType))
                return 0;

            if (Vulnerabilities.Contains(damageType))
                return amount * 2;

            if (Resistances.Contains(damageType))
                return Math.Max(1, amount / 2);

            return amount;
        }

        public string Describe(DamageType damageType)
        {
            if (Immunities.Contains(damageType))
                return "Immune";
            if (Vulnerabilities.Contains(damageType))
                return "Vulnerable";
            if (Resistances.Contains(damageType))
                return "Resistant";
            return "Normal";
        }
    }
}
