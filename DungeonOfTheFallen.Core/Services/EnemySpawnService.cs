using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public static class EnemySpawnService
    {
        public static IReadOnlyList<Enemy> CreateRoster(BiomeType biomeType, int floor)
        {
            var roster = biomeType switch
            {
                BiomeType.Forest => new[]
                {
                    EnemyFactory.Create(EnemyType.Goblin, "Goblin Stalker", floor),
                    EnemyFactory.Create(EnemyType.Spider, "Thorn Spider", floor),
                    EnemyFactory.Create(EnemyType.Orc, "Briar Orc", floor),
                    EnemyFactory.Create(EnemyType.Dragon, "Ancient Forest Drake", floor)
                },
                BiomeType.Crypt => new[]
                {
                    EnemyFactory.Create(EnemyType.Skeleton, "Bone Sentinel", floor),
                    EnemyFactory.Create(EnemyType.Zombie, "Crypt Walker", floor),
                    EnemyFactory.Create(EnemyType.Orc, "Grave Reaver", floor),
                    EnemyFactory.Create(EnemyType.Lich, "Lich King", floor)
                },
                BiomeType.Volcanic => new[]
                {
                    EnemyFactory.Create(EnemyType.Goblin, "Ash Goblin", floor),
                    EnemyFactory.Create(EnemyType.Troll, "Magma Troll", floor),
                    EnemyFactory.Create(EnemyType.Orc, "Cinder Orc", floor),
                    EnemyFactory.Create(EnemyType.DemonLord, "Infernal Warden", floor)
                },
                BiomeType.Underwater => new[]
                {
                    EnemyFactory.Create(EnemyType.Spider, "Tide Spider", floor),
                    EnemyFactory.Create(EnemyType.Zombie, "Drowned Husk", floor),
                    EnemyFactory.Create(EnemyType.Troll, "Coral Troll", floor),
                    EnemyFactory.Create(EnemyType.Dragon, "Leviathan Spawn", floor)
                },
                BiomeType.Celestial => new[]
                {
                    EnemyFactory.Create(EnemyType.Skeleton, "Fallen Sentinel", floor),
                    EnemyFactory.Create(EnemyType.Orc, "Astral Ravager", floor),
                    EnemyFactory.Create(EnemyType.Lich, "Radiant Lich", floor),
                    EnemyFactory.Create(EnemyType.Boss, "Seraphic Tyrant", floor)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(biomeType), biomeType, null)
            };

            return roster;
        }
    }
}
