namespace DungeonOfTheFallen.Core.Models
{
    public static class Dice
    {
        private static readonly Random _random = new();

        public static int Roll(int sides)
        {
            if (sides <= 0)
                throw new ArgumentOutOfRangeException(nameof(sides), "Dice sides must be greater than zero.");

            return _random.Next(1, sides + 1);
        }

        public static int Roll(DieSize dieSize) => Roll((int)dieSize);

        public static int Roll(int count, int sides)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Dice count must be greater than zero.");

            var total = 0;
            for (var i = 0; i < count; i++)
                total += Roll(sides);

            return total;
        }

        public static int Roll(int count, DieSize dieSize) => Roll(count, (int)dieSize);

        public static int RollD4() => Roll(4);
        public static int RollD6() => Roll(6);
        public static int RollD8() => Roll(8);
        public static int RollD10() => Roll(10);
        public static int RollD12() => Roll(12);
        public static int RollD20() => Roll(20);

        public static bool IsNatural20(int d20Roll) => d20Roll == 20;
        public static bool IsNatural1(int d20Roll) => d20Roll == 1;
    }
}
