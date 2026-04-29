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
                    DisplayName = "Krieger",
                    Description = "Gepanzerter Kämpfer mit konstantem d8-Waffenschaden.",
                    MaxHP = 70,
                    Attack = 5,
                    Defense = 15,
                    StartingPotions = 1,
                    AccentColorHex = "#4F83FF",
                    StartingWeapon = new WeaponProfile { Name = "Langschwert", AttackBonus = 2, ArmorClassBonus = 1, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D8, Bonus = 3, DamageType = DamageType.Slashing } },
                    ClassSkill = new PlayerSkill { Name = "Schildhieb", Description = "+2 Trefferwurf, +1W6 Wuchtschaden, +2 RK für den Gegenzug.", AttackBonus = 2, BonusDamageDiceCount = 1, BonusDamageDie = DieSize.D6, BonusDamageType = DamageType.Arcane, DefenseBoost = 2 }
                },
                PlayerClass.Rogue => new PlayerClassProfile
                {
                    PlayerClass = playerClass,
                    DisplayName = "Schurke",
                    Description = "Präziser Angreifer mit leichter Rüstung und Stich-Kritik.",
                    MaxHP = 52,
                    Attack = 6,
                    Defense = 13,
                    StartingPotions = 2,
                    AccentColorHex = "#7A4DFF",
                    StartingWeapon = new WeaponProfile { Name = "Rapier", AttackBonus = 3, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D8, Bonus = 2, DamageType = DamageType.Piercing } },
                    ClassSkill = new PlayerSkill { Name = "Hinterhalt", Description = "+3 Trefferwurf und +2W4 Stichschaden.", AttackBonus = 3, BonusDamageDiceCount = 2, BonusDamageDie = DieSize.D4, BonusDamageType = DamageType.Piercing }
                },
                PlayerClass.Mage => new PlayerClassProfile
                {
                    PlayerClass = playerClass,
                    DisplayName = "Magier",
                    Description = "Glaskanone mit arkanem W10-Schlag.",
                    MaxHP = 44,
                    Attack = 5,
                    Defense = 11,
                    StartingPotions = 2,
                    AccentColorHex = "#8B5CFF",
                    StartingWeapon = new WeaponProfile { Name = "Arkaner Fokus", AttackBonus = 3, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D10, Bonus = 1, DamageType = DamageType.Arcane } },
                    DamageModifiers = new DamageModifierSet { Resistances = { DamageType.Arcane } },
                    ClassSkill = new PlayerSkill { Name = "Arkaner Stoß", Description = "+1W8 Arkanzschaden und +6 HP heilen.", BonusDamageDiceCount = 1, BonusDamageDie = DieSize.D8, BonusDamageType = DamageType.Arcane, Healing = 6 }
                },
                PlayerClass.Cleric => new PlayerClassProfile
                {
                    PlayerClass = playerClass,
                    DisplayName = "Kleriker",
                    Description = "Zäher Unterstützer, gestärkt durch strahlendes Licht.",
                    MaxHP = 58,
                    Attack = 4,
                    Defense = 14,
                    StartingPotions = 3,
                    AccentColorHex = "#F5D76E",
                    StartingWeapon = new WeaponProfile { Name = "Geweihter Streitkolben", AttackBonus = 2, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D6, Bonus = 3, DamageType = DamageType.Bludgeoning } },
                    DamageModifiers = new DamageModifierSet { Resistances = { DamageType.Necrotic } },
                    ClassSkill = new PlayerSkill { Name = "Lichtgebet", Description = "+12 HP Heilung, +1W6 Strahlenschaden, +1 RK.", BonusDamageDiceCount = 1, BonusDamageDie = DieSize.D6, BonusDamageType = DamageType.Radiant, Healing = 12, DefenseBoost = 1 }
                },
                PlayerClass.Ranger => new PlayerClassProfile
                {
                    PlayerClass = playerClass,
                    DisplayName = "Waldläufer",
                    Description = "Beständiger Jäger mit Fernkampf-W8-Treffern.",
                    MaxHP = 56,
                    Attack = 5,
                    Defense = 13,
                    StartingPotions = 2,
                    AccentColorHex = "#4CAF50",
                    StartingWeapon = new WeaponProfile { Name = "Langbogen", AttackBonus = 3, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D8, Bonus = 2, DamageType = DamageType.Piercing } },
                    ClassSkill = new PlayerSkill { Name = "Pfeilhagel", Description = "+2 Trefferwurf und +1W8 Stichschaden.", AttackBonus = 2, BonusDamageDiceCount = 1, BonusDamageDie = DieSize.D8, BonusDamageType = DamageType.Piercing }
                },
                PlayerClass.Assassin => new PlayerClassProfile
                {
                    PlayerClass = playerClass,
                    DisplayName = "Assassine",
                    Description = "Hochrisiko-Burst-Spezialist mit vernichtenden Kritrunden.",
                    MaxHP = 48,
                    Attack = 6,
                    Defense = 12,
                    StartingPotions = 1,
                    AccentColorHex = "#D94B4B",
                    StartingWeapon = new WeaponProfile { Name = "Zwillingsdolche", AttackBonus = 4, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D6, Bonus = 3, DamageType = DamageType.Piercing } },
                    ClassSkill = new PlayerSkill { Name = "Hinrichtung", Description = "+4 Trefferwurf und +2W6 Nekrotenschaden.", AttackBonus = 4, BonusDamageDiceCount = 2, BonusDamageDie = DieSize.D6, BonusDamageType = DamageType.Necrotic }
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

            var startingPotions = Math.Max(profile.StartingPotions, GameBalance.MinimumStartingPotions);
            for (var i = 0; i < startingPotions; i++)
                player.Inventory.Add(new Potion("Heiltrank", 25));
        }
    }
}
