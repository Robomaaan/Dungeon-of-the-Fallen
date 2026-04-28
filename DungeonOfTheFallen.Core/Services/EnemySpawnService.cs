using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public static class EnemySpawnService
    {
        public static IReadOnlyList<Enemy> CreateRoster(BiomeType biomeType, int floor)
        {
            var roster = biomeType switch
            {
                // Erster Boss: Grondak der Knochenbrecher (Ogre)
                BiomeType.Forest => new[]
                {
                    EnemyFactory.Create(EnemyType.Goblin,  "Goblin-Aufseher",           floor),
                    EnemyFactory.Create(EnemyType.Spider,  "Dornenspinne",              floor),
                    EnemyFactory.Create(EnemyType.Orc,     "Dickicht-Ork",              floor),
                    EnemyFactory.Create(EnemyType.Ogre,    "Grondak der Knochenbrecher", floor)
                },
                BiomeType.Crypt => new[]
                {
                    EnemyFactory.Create(EnemyType.Skeleton, "Knochenwächter",   floor),
                    EnemyFactory.Create(EnemyType.Zombie,   "Gruftgänger",      floor),
                    EnemyFactory.Create(EnemyType.Orc,      "Grabräuber",       floor),
                    EnemyFactory.Create(EnemyType.Lich,     "Lichkönig",        floor)
                },
                BiomeType.Volcanic => new[]
                {
                    EnemyFactory.Create(EnemyType.Goblin,    "Aschegoblin",       floor),
                    EnemyFactory.Create(EnemyType.Troll,     "Magmatroll",        floor),
                    EnemyFactory.Create(EnemyType.Orc,       "Glutork",           floor),
                    EnemyFactory.Create(EnemyType.DemonLord, "Höllenwächter",     floor)
                },
                BiomeType.Underwater => new[]
                {
                    EnemyFactory.Create(EnemyType.Spider, "Gezeitenspinne",  floor),
                    EnemyFactory.Create(EnemyType.Zombie, "Ertrunkener",     floor),
                    EnemyFactory.Create(EnemyType.Troll,  "Korallentroll",   floor),
                    EnemyFactory.Create(EnemyType.Dragon, "Leviathan-Brut",  floor)
                },
                BiomeType.Celestial => new[]
                {
                    EnemyFactory.Create(EnemyType.Skeleton, "Gefallener Wächter", floor),
                    EnemyFactory.Create(EnemyType.Orc,      "Astraler Verwüster", floor),
                    EnemyFactory.Create(EnemyType.Dragon,   "Sternendrache",      floor),
                    EnemyFactory.Create(EnemyType.Boss,     "Himmelsfürst",       floor)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(biomeType), biomeType, null)
            };

            return roster;
        }
    }
}
