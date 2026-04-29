using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public static class GameBalance
    {
        public const int MinimumStartingPotions = 2;

        public const int FloorOneNormalEnemyHpPenalty = 4;
        public const int FloorOneNormalEnemyAttackPenalty = 1;
        public const int FloorOneNormalEnemyWeaponAttackPenalty = 1;
        public const int FloorOneNormalEnemyDamagePenalty = 1;

        public const int FloorOneBossHpPenalty = 18;
        public const int FloorOneBossAttackPenalty = 3;
        public const int FloorOneBossWeaponAttackPenalty = 1;
        public const int FloorOneBossDamagePenalty = 2;

        public const double PostCombatHealPercent = 0.15;
        public const int PostCombatHealMinimum = 8;
        public const int PostCombatHealMaximum = 18;

        public const int FloorTransitionHealBase = 12;
        public const int FloorTransitionHealPerFloor = 4;

        public static int CalculatePostCombatHeal(Player player)
        {
            var heal = (int)Math.Round(player.MaxHP * PostCombatHealPercent, MidpointRounding.AwayFromZero);
            return Math.Clamp(heal, PostCombatHealMinimum, PostCombatHealMaximum);
        }

        public static int CalculateFloorTransitionHeal(Player player, int currentFloor)
        {
            var heal = FloorTransitionHealBase + (currentFloor * FloorTransitionHealPerFloor);
            return Math.Max(0, Math.Min(heal, player.MaxHP - player.HP));
        }
    }
}
