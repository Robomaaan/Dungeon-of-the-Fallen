using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public static class EnemyFactory
    {
        private sealed record EnemyTemplate(
            EnemyType Type,
            string Name,
            EnemyTier Tier,
            EnemyRole Role,
            int MaxHP,
            int AttackBonus,
            int ArmorClass,
            int Gold,
            int Xp,
            WeaponProfile Weapon,
            DamageModifierSet? Modifiers = null,
            bool IsBoss = false);

        private static readonly Dictionary<EnemyType, EnemyTemplate> Templates = new()
        {
            [EnemyType.Goblin] = new(EnemyType.Goblin, "Goblin Skirmisher", EnemyTier.Skirmisher, EnemyRole.Scout, 24, 4, 12, 45, 80, new WeaponProfile { Name = "Rust Knife", AttackBonus = 2, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D6, Bonus = 1, DamageType = DamageType.Piercing } }),
            [EnemyType.Spider] = new(EnemyType.Spider, "Venom Spider", EnemyTier.Skirmisher, EnemyRole.Scout, 22, 5, 13, 50, 90, new WeaponProfile { Name = "Venom Fangs", AttackBonus = 2, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D4, Bonus = 3, DamageType = DamageType.Poison } }, new DamageModifierSet { Resistances = { DamageType.Poison } }),
            [EnemyType.Skeleton] = new(EnemyType.Skeleton, "Skeleton Guard", EnemyTier.Veteran, EnemyRole.Soldier, 30, 4, 14, 60, 110, new WeaponProfile { Name = "Bone Sword", AttackBonus = 2, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D8, Bonus = 2, DamageType = DamageType.Slashing } }, new DamageModifierSet { Resistances = { DamageType.Piercing }, Vulnerabilities = { DamageType.Bludgeoning }, Immunities = { DamageType.Poison } }),
            [EnemyType.Orc] = new(EnemyType.Orc, "Orc Raider", EnemyTier.Veteran, EnemyRole.Brute, 38, 5, 13, 80, 135, new WeaponProfile { Name = "Greataxe", AttackBonus = 2, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D12, Bonus = 2, DamageType = DamageType.Slashing } }),
            [EnemyType.Zombie] = new(EnemyType.Zombie, "Rotting Zombie", EnemyTier.Veteran, EnemyRole.Brute, 34, 3, 11, 70, 120, new WeaponProfile { Name = "Slam", AttackBonus = 2, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D8, Bonus = 2, DamageType = DamageType.Bludgeoning } }, new DamageModifierSet { Resistances = { DamageType.Necrotic }, Vulnerabilities = { DamageType.Radiant } }),
            [EnemyType.Troll] = new(EnemyType.Troll, "Cave Troll", EnemyTier.Elite, EnemyRole.Brute, 52, 6, 15, 120, 180, new WeaponProfile { Name = "Massive Club", AttackBonus = 3, Damage = new DamageRoll { Count = 2, DieSize = DieSize.D6, Bonus = 2, DamageType = DamageType.Bludgeoning } }, new DamageModifierSet { Vulnerabilities = { DamageType.Fire } }),
            [EnemyType.Dragon] = new(EnemyType.Dragon, "Ancient Dragon", EnemyTier.Boss, EnemyRole.Boss, 74, 7, 17, 300, 320, new WeaponProfile { Name = "Flame Breath", AttackBonus = 4, Damage = new DamageRoll { Count = 2, DieSize = DieSize.D10, Bonus = 3, DamageType = DamageType.Fire } }, new DamageModifierSet { Resistances = { DamageType.Fire } }, true),
            [EnemyType.DemonLord] = new(EnemyType.DemonLord, "Demon Lord", EnemyTier.Boss, EnemyRole.Boss, 70, 8, 17, 320, 340, new WeaponProfile { Name = "Hellblade", AttackBonus = 4, Damage = new DamageRoll { Count = 2, DieSize = DieSize.D8, Bonus = 4, DamageType = DamageType.Necrotic } }, new DamageModifierSet { Resistances = { DamageType.Fire, DamageType.Necrotic } }, true),
            [EnemyType.Lich] = new(EnemyType.Lich, "Lich Sovereign", EnemyTier.Boss, EnemyRole.Boss, 64, 8, 16, 290, 330, new WeaponProfile { Name = "Soul Bolt", AttackBonus = 4, Damage = new DamageRoll { Count = 2, DieSize = DieSize.D8, Bonus = 3, DamageType = DamageType.Arcane } }, new DamageModifierSet { Resistances = { DamageType.Cold, DamageType.Necrotic }, Vulnerabilities = { DamageType.Radiant } }, true),
            [EnemyType.Boss] = new(EnemyType.Boss, "Dungeon Tyrant", EnemyTier.Boss, EnemyRole.Boss, 80, 8, 18, 360, 360, new WeaponProfile { Name = "Tyrant's Claw", AttackBonus = 4, Damage = new DamageRoll { Count = 2, DieSize = DieSize.D10, Bonus = 4, DamageType = DamageType.Slashing } }, null, true)
        };

        public static Enemy Create(EnemyType type, string? name = null, int floor = 1)
        {
            var t = Templates[type];
            var scale = Math.Max(0, floor - 1);
            var enemy = new Enemy(name ?? t.Name, t.Type, t.MaxHP + (scale * (t.IsBoss ? 8 : 4)), t.AttackBonus + (scale / 2), t.ArmorClass + (scale / 2), t.IsBoss)
            {
                GoldReward = t.Gold + (scale * 15),
                XpReward = t.Xp + (scale * 25),
                Tier = t.Tier,
                Role = t.Role,
                Weapon = new WeaponProfile
                {
                    Name = t.Weapon.Name,
                    AttackBonus = t.Weapon.AttackBonus,
                    ArmorClassBonus = t.Weapon.ArmorClassBonus,
                    Damage = new DamageRoll
                    {
                        Count = t.Weapon.Damage.Count,
                        DieSize = t.Weapon.Damage.DieSize,
                        Bonus = t.Weapon.Damage.Bonus + (t.IsBoss ? scale : scale / 2),
                        DamageType = t.Weapon.Damage.DamageType,
                        Label = t.Weapon.Damage.Label
                    }
                },
                DamageModifiers = t.Modifiers ?? new DamageModifierSet()
            };

            return enemy;
        }
    }
}
