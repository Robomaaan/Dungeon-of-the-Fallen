using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public static class EnemyFactory
    {
        public static Enemy CreateGoblin(string name = "Goblin") =>
            new(name, EnemyType.Goblin, maxHP: 20, attack: 5, defense: 2);

        public static Enemy CreateOrc(string name = "Orc") =>
            new(name, EnemyType.Orc, maxHP: 35, attack: 8, defense: 4);

        public static Enemy CreateBoss(string name = "Dragon Boss") =>
            new(name, EnemyType.Boss, maxHP: 80, attack: 15, defense: 6, isBoss: true);
    }
}
