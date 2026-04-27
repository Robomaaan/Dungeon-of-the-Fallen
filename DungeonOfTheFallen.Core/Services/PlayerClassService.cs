using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public static class PlayerClassService
    {
        public static IReadOnlyList<PlayerClassProfile> GetAllProfiles() =>
            Enum.GetValues<PlayerClass>().Select(GetProfile).ToList();

        public static PlayerClassProfile GetProfile(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Warrior => new PlayerClassProfile
                {
                    PlayerClass = playerClass,
                    DisplayName = "Warrior",
                    Description = "Armored frontliner with consistent d8 weapon damage.",
                    MaxHP = 70,
                    Attack = 5,
                    Defense = 15,
                    StartingPotions = 1,
                    AccentColorHex = "#4F83FF",
                    StartingWeapon = new WeaponProfile { Name = "Longsword", AttackBonus = 2, ArmorClassBonus = 1, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D8, Bonus = 3, DamageType = DamageType.Slashing } },
                    ClassSkill = new PlayerSkill { Name = "Shield Slam", Description = "+2 to hit, +1d6 force damage, +2 AC for the enemy turn.", AttackBonus = 2, BonusDamageDiceCount = 1, BonusDamageDie = DieSize.D6, BonusDamageType = DamageType.Arcane, DefenseBoost = 2 }
                },
                PlayerClass.Rogue => new PlayerClassProfile
                {
                    PlayerClass = playerClass,
                    DisplayName = "Rogue",
                    Description = "Precise striker with light armor and piercing crit pressure.",
                    MaxHP = 52,
                    Attack = 6,
                    Defense = 13,
                    StartingPotions = 2,
                    AccentColorHex = "#7A4DFF",
                    StartingWeapon = new WeaponProfile { Name = "Rapier", AttackBonus = 3, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D8, Bonus = 2, DamageType = DamageType.Piercing } },
                    ClassSkill = new PlayerSkill { Name = "Backstab", Description = "+3 to hit and +2d4 piercing damage.", AttackBonus = 3, BonusDamageDiceCount = 2, BonusDamageDie = DieSize.D4, BonusDamageType = DamageType.Piercing }
                },
                PlayerClass.Mage => new PlayerClassProfile
                {
                    PlayerClass = playerClass,
                    DisplayName = "Mage",
                    Description = "Glass cannon with arcane d10 bursts.",
                    MaxHP = 44,
                    Attack = 5,
                    Defense = 11,
                    StartingPotions = 2,
                    AccentColorHex = "#8B5CFF",
                    StartingWeapon = new WeaponProfile { Name = "Arcane Focus", AttackBonus = 3, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D10, Bonus = 1, DamageType = DamageType.Arcane } },
                    DamageModifiers = new DamageModifierSet { Resistances = { DamageType.Arcane } },
                    ClassSkill = new PlayerSkill { Name = "Arcane Burst", Description = "+1d8 arcane damage and heal 6 HP.", BonusDamageDiceCount = 1, BonusDamageDie = DieSize.D8, BonusDamageType = DamageType.Arcane, Healing = 6 }
                },
                PlayerClass.Cleric => new PlayerClassProfile
                {
                    PlayerClass = playerClass,
                    DisplayName = "Cleric",
                    Description = "Durable support empowered by radiant magic.",
                    MaxHP = 58,
                    Attack = 4,
                    Defense = 14,
                    StartingPotions = 3,
                    AccentColorHex = "#F5D76E",
                    StartingWeapon = new WeaponProfile { Name = "Blessed Mace", AttackBonus = 2, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D6, Bonus = 3, DamageType = DamageType.Bludgeoning } },
                    DamageModifiers = new DamageModifierSet { Resistances = { DamageType.Necrotic } },
                    ClassSkill = new PlayerSkill { Name = "Radiant Prayer", Description = "Heal 12 HP, +1d6 radiant damage, +1 AC.", BonusDamageDiceCount = 1, BonusDamageDie = DieSize.D6, BonusDamageType = DamageType.Radiant, Healing = 12, DefenseBoost = 1 }
                },
                PlayerClass.Ranger => new PlayerClassProfile
                {
                    PlayerClass = playerClass,
                    DisplayName = "Ranger",
                    Description = "Steady hunter with long-range style d8 hits.",
                    MaxHP = 56,
                    Attack = 5,
                    Defense = 13,
                    StartingPotions = 2,
                    AccentColorHex = "#4CAF50",
                    StartingWeapon = new WeaponProfile { Name = "Longbow", AttackBonus = 3, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D8, Bonus = 2, DamageType = DamageType.Piercing } },
                    ClassSkill = new PlayerSkill { Name = "Piercing Volley", Description = "+2 to hit and +1d8 piercing damage.", AttackBonus = 2, BonusDamageDiceCount = 1, BonusDamageDie = DieSize.D8, BonusDamageType = DamageType.Piercing }
                },
                PlayerClass.Assassin => new PlayerClassProfile
                {
                    PlayerClass = playerClass,
                    DisplayName = "Assassin",
                    Description = "High-risk burst specialist with vicious crit turns.",
                    MaxHP = 48,
                    Attack = 6,
                    Defense = 12,
                    StartingPotions = 1,
                    AccentColorHex = "#D94B4B",
                    StartingWeapon = new WeaponProfile { Name = "Twin Daggers", AttackBonus = 4, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D6, Bonus = 3, DamageType = DamageType.Piercing } },
                    ClassSkill = new PlayerSkill { Name = "Execution", Description = "+4 to hit and +2d6 necrotic damage.", AttackBonus = 4, BonusDamageDiceCount = 2, BonusDamageDie = DieSize.D6, BonusDamageType = DamageType.Necrotic }
                },
                _ => throw new ArgumentOutOfRangeException(nameof(playerClass), playerClass, null)
            };
        }

        public static void ApplyClass(Player player, PlayerClass playerClass)
        {
            var profile = GetProfile(playerClass);
            player.PlayerClass = playerClass;
            player.Name = profile.DisplayName;
            player.MaxHP = profile.MaxHP;
            player.HP = profile.MaxHP;
            player.Attack = profile.Attack;
            player.Defense = profile.Defense;
            player.Weapon = profile.StartingWeapon;
            player.DamageModifiers = profile.DamageModifiers;
            player.Inventory.Clear();
            player.ClassSkill = profile.ClassSkill;

            for (var i = 0; i < profile.StartingPotions; i++)
                player.Inventory.Add(new Potion("Health Potion", 25));
        }
    }
}
