using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public static class PlayerProgressionService
    {
        public static bool GrantXp(Player player, int xpAmount, IList<string>? messages = null)
        {
            if (xpAmount <= 0)
                return false;

            player.XP += xpAmount;
            var leveled = false;

            while (player.XP >= player.Level * 200)
            {
                player.Level++;
                player.MaxHP += 10;
                player.HP = player.MaxHP;
                player.Attack += 2;
                player.Defense += 1;
                leveled = true;
                messages?.Add($"[LEVEL UP] Du bist jetzt Level {player.Level}!");
            }

            return leveled;
        }
    }
}
