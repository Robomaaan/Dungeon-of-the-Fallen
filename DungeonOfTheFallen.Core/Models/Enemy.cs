namespace DungeonOfTheFallen.Core.Models
{
    public class Enemy
    {
        public string Name { get; set; }
        public EnemyType EnemyType { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public CombatantStats Stats { get; set; }
        public bool IsBoss { get; set; }
        public int GoldReward { get; set; }
        public int XpReward { get; set; }
        public EnemyTier Tier { get; set; }
        public EnemyRole Role { get; set; }

        public int HP { get => Stats.HP; set => Stats.HP = value; }
        public int MaxHP { get => Stats.MaxHP; set => Stats.MaxHP = value; }
        public int Attack { get => Stats.AttackBonus; set => Stats.AttackBonus = value; }
        public int Defense { get => Stats.ArmorClass; set => Stats.ArmorClass = value; }
        public WeaponProfile Weapon { get => Stats.Weapon; set => Stats.Weapon = value; }
        public DamageModifierSet DamageModifiers { get => Stats.DamageModifiers; set => Stats.DamageModifiers = value; }
        public int ArmorClass => Defense + Weapon.ArmorClassBonus;

        public Enemy(string name, EnemyType type, int maxHP = 20, int attack = 3, int defense = 10, bool isBoss = false)
        {
            Name = name;
            EnemyType = type;
            Stats = new CombatantStats
            {
                MaxHP = maxHP,
                HP = maxHP,
                AttackBonus = attack,
                ArmorClass = defense,
                Weapon = new WeaponProfile
                {
                    Name = "Claws",
                    Damage = new DamageRoll { Count = 1, DieSize = DieSize.D6, Bonus = 1, DamageType = DamageType.Slashing }
                }
            };
            IsBoss = isBoss;
            Tier = isBoss ? EnemyTier.Boss : EnemyTier.Skirmisher;
            Role = isBoss ? EnemyRole.Boss : EnemyRole.Soldier;
            GoldReward = isBoss ? 500 : 50;
            XpReward = isBoss ? 200 : 25;
        }

        public bool IsAlive => HP > 0;
    }
}
