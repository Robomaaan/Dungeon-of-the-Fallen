namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    public record Room(string Id, string DisplayName, int X1, int Y1, int X2, int Y2)
    {
        public int Width  => X2 - X1 + 1;
        public int Height => Y2 - Y1 + 1;
        public int CenterX => (X1 + X2) / 2;
        public int CenterY => (Y1 + Y2) / 2;
        public bool Contains(int x, int y) => x >= X1 && x <= X2 && y >= Y1 && y <= Y2;
    }
}
