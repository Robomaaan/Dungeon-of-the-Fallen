using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public static class EnemyFactory
    {
        // ── Scaling-Konstanten (hier für späteres Balancing anpassen) ──────────
        // HP pro Etagenstufe (floor - 1) für normale Gegner
        private const int HpScalePerFloor        = 4;
        // HP pro Etagenstufe für Boss-Gegner
        private const int BossHpScalePerFloor     = 8;
        // Angriffs-/RK-Bonus pro zwei Etagenstufen (floor - 1) / 2
        private const double AttackAcScaleStep    = 0.5;
        // Schadenswurf-Bonus pro Etagenstufe für Boss-Gegner
        private const int BossDmgBonusPerFloor    = 1;
        // Schadenswurf-Bonus pro zwei Etagenstufen für normale Gegner
        private const double MinionDmgBonusStep   = 0.5;
        // Gold-Bonus pro Etagenstufe
        private const int GoldBonusPerFloor       = 15;
        // XP-Bonus pro Etagenstufe
        private const int XpBonusPerFloor         = 25;
        // ──────────────────────────────────────────────────────────────────────

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
            [EnemyType.Goblin] = new(EnemyType.Goblin, "Goblin-Plänkler", EnemyTier.Skirmisher, EnemyRole.Scout, 24, 4, 12, 45, 80, new WeaponProfile { Name = "Rostmesser", AttackBonus = 2, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D6, Bonus = 1, DamageType = DamageType.Piercing } }),
            [EnemyType.Spider] = new(EnemyType.Spider, "Giftspinne", EnemyTier.Skirmisher, EnemyRole.Scout, 22, 5, 13, 50, 90, new WeaponProfile { Name = "Giftzähne", AttackBonus = 2, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D4, Bonus = 3, DamageType = DamageType.Poison } }, new DamageModifierSet { Resistances = { DamageType.Poison } }),
            [EnemyType.Skeleton] = new(EnemyType.Skeleton, "Knochensoldat", EnemyTier.Veteran, EnemyRole.Soldier, 30, 4, 14, 60, 110, new WeaponProfile { Name = "Knochenschwert", AttackBonus = 2, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D8, Bonus = 2, DamageType = DamageType.Slashing } }, new DamageModifierSet { Resistances = { DamageType.Piercing }, Vulnerabilities = { DamageType.Bludgeoning }, Immunities = { DamageType.Poison } }),
            [EnemyType.Orc] = new(EnemyType.Orc, "Ork-Plünderer", EnemyTier.Veteran, EnemyRole.Brute, 38, 5, 13, 80, 135, new WeaponProfile { Name = "Großaxt", AttackBonus = 2, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D12, Bonus = 2, DamageType = DamageType.Slashing } }),
            [EnemyType.Zombie] = new(EnemyType.Zombie, "Faulender Zombie", EnemyTier.Veteran, EnemyRole.Brute, 34, 3, 11, 70, 120, new WeaponProfile { Name = "Schlag", AttackBonus = 2, Damage = new DamageRoll { Count = 1, DieSize = DieSize.D8, Bonus = 2, DamageType = DamageType.Bludgeoning } }, new DamageModifierSet { Resistances = { DamageType.Necrotic }, Vulnerabilities = { DamageType.Radiant } }),
            [EnemyType.Troll] = new(EnemyType.Troll, "Höhlentroll", EnemyTier.Elite, EnemyRole.Brute, 52, 6, 15, 120, 180, new WeaponProfile { Name = "Riesige Keule", AttackBonus = 3, Damage = new DamageRoll { Count = 2, DieSize = DieSize.D6, Bonus = 2, DamageType = DamageType.Bludgeoning } }, new DamageModifierSet { Vulnerabilities = { DamageType.Fire } }),
            [EnemyType.Ogre] = new(EnemyType.Ogre, "Grondak der Knochenbrecher", EnemyTier.Boss, EnemyRole.Boss, 80, 7, 16, 300, 330, new WeaponProfile { Name = "Steinkeule", AttackBonus = 4, Damage = new DamageRoll { Count = 2, DieSize = DieSize.D8, Bonus = 4, DamageType = DamageType.Bludgeoning, Label = "Wuchtschaden" } }, new DamageModifierSet { Resistances = { DamageType.Bludgeoning } }, true),
            [EnemyType.Dragon] = new(EnemyType.Dragon, "Uralter Drache", EnemyTier.Boss, EnemyRole.Boss, 74, 7, 17, 300, 320, new WeaponProfile { Name = "Flammenodem", AttackBonus = 4, Damage = new DamageRoll { Count = 2, DieSize = DieSize.D10, Bonus = 3, DamageType = DamageType.Fire } }, new DamageModifierSet { Resistances = { DamageType.Fire } }, true),
            [EnemyType.DemonLord] = new(EnemyType.DemonLord, "Teufelsfürst", EnemyTier.Boss, EnemyRole.Boss, 70, 8, 17, 320, 340, new WeaponProfile { Name = "Höllenklinge", AttackBonus = 4, Damage = new DamageRoll { Count = 2, DieSize = DieSize.D8, Bonus = 4, DamageType = DamageType.Necrotic } }, new DamageModifierSet { Resistances = { DamageType.Fire, DamageType.Necrotic } }, true),
            [EnemyType.Lich] = new(EnemyType.Lich, "Lich-Souverän", EnemyTier.Boss, EnemyRole.Boss, 64, 8, 16, 290, 330, new WeaponProfile { Name = "Seelenblitz", AttackBonus = 4, Damage = new DamageRoll { Count = 2, DieSize = DieSize.D8, Bonus = 3, DamageType = DamageType.Arcane } }, new DamageModifierSet { Resistances = { DamageType.Cold, DamageType.Necrotic }, Vulnerabilities = { DamageType.Radiant } }, true),
            [EnemyType.Boss] = new(EnemyType.Boss, "Kerkertyrann", EnemyTier.Boss, EnemyRole.Boss, 80, 8, 18, 360, 360, new WeaponProfile { Name = "Tyrannenklau", AttackBonus = 4, Damage = new DamageRoll { Count = 2, DieSize = DieSize.D10, Bonus = 4, DamageType = DamageType.Slashing } }, null, true)
        };

        public static Enemy Create(EnemyType type, string? name = null, int floor = 1)
        {
            var t     = Templates[type];
            var scale = Math.Max(0, floor - 1);

            var hpBonus     = scale * (t.IsBoss ? BossHpScalePerFloor  : HpScalePerFloor);
            var attackBonus = (int)(scale * AttackAcScaleStep);
            var acBonus     = (int)(scale * AttackAcScaleStep);
            var dmgBonus    = t.IsBoss ? scale * BossDmgBonusPerFloor : (int)(scale * MinionDmgBonusStep);

            var enemy = new Enemy(name ?? t.Name, t.Type, t.MaxHP + hpBonus, t.AttackBonus + attackBonus, t.ArmorClass + acBonus, t.IsBoss)
            {
                GoldReward = t.Gold + (scale * GoldBonusPerFloor),
                XpReward   = t.Xp   + (scale * XpBonusPerFloor),
                Tier   = t.Tier,
                Role   = t.Role,
                Weapon = new WeaponProfile
                {
                    Name            = t.Weapon.Name,
                    AttackBonus     = t.Weapon.AttackBonus,
                    ArmorClassBonus = t.Weapon.ArmorClassBonus,
                    Damage = new DamageRoll
                    {
                        Count      = t.Weapon.Damage.Count,
                        DieSize    = t.Weapon.Damage.DieSize,
                        Bonus      = t.Weapon.Damage.Bonus + dmgBonus,
                        DamageType = t.Weapon.Damage.DamageType,
                        Label      = t.Weapon.Damage.Label
                    }
                },
                    DamageModifiers = t.Modifiers ?? new DamageModifierSet()
                };

            if (floor == 1)
                ApplyFloorOneBalance(enemy);

            return enemy;
        }

        private static void ApplyFloorOneBalance(Enemy enemy)
        {
            if (enemy.IsBoss)
            {
                enemy.MaxHP = Math.Max(1, enemy.MaxHP - GameBalance.FloorOneBossHpPenalty);
                enemy.HP = enemy.MaxHP;
                enemy.Attack = Math.Max(0, enemy.Attack - GameBalance.FloorOneBossAttackPenalty);
                enemy.Weapon.AttackBonus = Math.Max(0, enemy.Weapon.AttackBonus - GameBalance.FloorOneBossWeaponAttackPenalty);
                enemy.Weapon.Damage.Bonus = Math.Max(0, enemy.Weapon.Damage.Bonus - GameBalance.FloorOneBossDamagePenalty);
                return;
            }

            enemy.MaxHP = Math.Max(1, enemy.MaxHP - GameBalance.FloorOneNormalEnemyHpPenalty);
            enemy.HP = enemy.MaxHP;
            enemy.Attack = Math.Max(0, enemy.Attack - GameBalance.FloorOneNormalEnemyAttackPenalty);
            enemy.Weapon.AttackBonus = Math.Max(0, enemy.Weapon.AttackBonus - GameBalance.FloorOneNormalEnemyWeaponAttackPenalty);
            enemy.Weapon.Damage.Bonus = Math.Max(0, enemy.Weapon.Damage.Bonus - GameBalance.FloorOneNormalEnemyDamagePenalty);
        }
    }
}
