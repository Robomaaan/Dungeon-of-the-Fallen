namespace DungeonOfTheFallen.Core.Models
{
    public class Enemy
    {
        public string Name { get; set; }
        public EnemyType EnemyType { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public bool IsBoss { get; set; }
        public int GoldReward { get; set; }
        public int XpReward { get; set; }

        public Enemy(string name, EnemyType type, int maxHP = 20, int attack = 5, int defense = 2, bool isBoss = false)
        {
            Name = name;
            EnemyType = type;
            MaxHP = maxHP;
            HP = maxHP;
            Attack = attack;
            Defense = defense;
            IsBoss = isBoss;
            GoldReward = isBoss ? 500 : 50;
            XpReward = isBoss ? 200 : 25;
        }

        public bool IsAlive => HP > 0;
    }
}
