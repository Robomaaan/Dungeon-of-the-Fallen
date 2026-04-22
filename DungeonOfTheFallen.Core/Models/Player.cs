namespace DungeonOfTheFallen.Core.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int XP { get; set; }
        public int Level { get; set; }
        public int Gold { get; set; }
        public Inventory Inventory { get; set; }

        public Player(string name = "Hero")
        {
            Name = name;
            PositionX = 0;
            PositionY = 0;
            MaxHP = 50;
            HP = MaxHP;
            Attack = 10;
            Defense = 5;
            XP = 0;
            Level = 1;
            Gold = 0;
            Inventory = new Inventory();
        }

        public bool IsAlive => HP > 0;
    }
}
