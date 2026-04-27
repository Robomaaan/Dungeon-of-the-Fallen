using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public static class FloorThemeService
    {
        private static readonly BiomeType[] Rotation =
        {
            BiomeType.Forest,
            BiomeType.Crypt,
            BiomeType.Volcanic,
            BiomeType.Underwater,
            BiomeType.Celestial
        };

        public static BiomeType GetBiomeForFloor(int floor)
        {
            if (floor <= 0) floor = 1;
            return Rotation[(floor - 1) % Rotation.Length];
        }
    }
}
