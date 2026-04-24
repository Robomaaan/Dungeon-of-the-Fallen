namespace DungeonOfTheFallen.Core.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public CombatantStats Stats { get; set; }
        public int XP { get; set; }
        public int Level { get; set; }
        public int Gold { get; set; }
        public Inventory Inventory { get; set; }

        public int HP { get => Stats.HP; set => Stats.HP = value; }
        public int MaxHP { get => Stats.MaxHP; set => Stats.MaxHP = value; }
        public int Attack { get => Stats.Attack; set => Stats.Attack = value; }
        public int Defense { get => Stats.Defense; set => Stats.Defense = value; }

        public Player(string name = "Hero")
        {
            Name = name;
            PositionX = 0;
            PositionY = 0;
            Stats = new CombatantStats
            {
                MaxHP = 50,
                HP = 50,
                Attack = 10,
                Defense = 5
            };
            XP = 0;
            Level = 1;
            Gold = 0;
            Inventory = new Inventory();
        }

        public bool IsAlive => HP > 0;
    }
}
