namespace Projektarbeit_Dungeon_of_the_Fallen
{
    public class PlayerClassDefinition
    {
        public string Id               { get; set; } = "";
        public string DisplayName      { get; set; } = "";
        public string PortraitImage    { get; set; } = "";
        public string SpriteImage      { get; set; } = "";
        public string ShortDescription { get; set; } = "";
        public string HoverDescription { get; set; } = "";
        public List<string> Specials   { get; set; } = new();
        public ClassBaseStats BaseStats { get; set; } = new();
    }

    public class ClassBaseStats
    {
        public int Health   { get; set; }
        public int Damage   { get; set; }
        public int Defense  { get; set; }
        public int Speed    { get; set; }
        public int Magic    { get; set; }
    }
}
