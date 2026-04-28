namespace DungeonOfTheFallen.Core.Models
{
    public class Player
    {
        public string Name { get; set; }
        public PlayerClass PlayerClass { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public CombatantStats Stats { get; set; }
        public int XP { get; set; }
        public int Level { get; set; }
        public int Gold { get; set; }
        public Inventory Inventory { get; set; }
        public PlayerSkill ClassSkill { get; set; }

        public int HP { get => Stats.HP; set => Stats.HP = value; }
        public int MaxHP { get => Stats.MaxHP; set => Stats.MaxHP = value; }
        public int Attack { get => Stats.AttackBonus; set => Stats.AttackBonus = value; }
        public int Defense { get => Stats.ArmorClass; set => Stats.ArmorClass = value; }
        public WeaponProfile Weapon { get => Stats.Weapon; set => Stats.Weapon = value; }
        public DamageModifierSet DamageModifiers { get => Stats.DamageModifiers; set => Stats.DamageModifiers = value; }

        // Ausgerüstete Rüstungsstücke (Slot → Profil)
        public Dictionary<EquipmentSlot, ArmorProfile> EquippedArmor { get; set; } = new();

        // Gesamte Rüstungsklasse: Basis + Waffe + ausgerüstete Rüstung
        public int ArmorClass => Defense + Weapon.ArmorClassBonus + EquippedArmor.Values.Sum(a => a.ArmorValue);

        // Summe der Rüstungswerte aller ausgerüsteten Teile
        public int TotalArmorValue => EquippedArmor.Values.Sum(a => a.ArmorValue);

        public Player(string name = "Hero")
        {
            Name = name;
            PlayerClass = PlayerClass.Warrior;
            PositionX = 0;
            PositionY = 0;
            Stats = new CombatantStats
            {
                MaxHP = 50,
                HP = 50,
                AttackBonus = 5,
                ArmorClass = 12,
                Weapon = new WeaponProfile
                {
                    Name = "Training Sword",
                    AttackBonus = 2,
                    Damage = new DamageRoll { Count = 1, DieSize = DieSize.D8, Bonus = 2, DamageType = DamageType.Slashing, Label = "Slash" }
                }
            };
            XP = 0;
            Level = 1;
            Gold = 0;
            Inventory = new Inventory();
            ClassSkill = new PlayerSkill
            {
                Name = "Shield Slam",
                Description = "+2 attack roll, weapon damage +1d6 force, and +2 AC for the counterstrike.",
                AttackBonus = 2,
                BonusDamageDiceCount = 1,
                BonusDamageDie = DieSize.D6,
                BonusDamageType = DamageType.Arcane,
                DefenseBoost = 2
            };
        }

        public bool IsAlive => HP > 0;
    }
}
