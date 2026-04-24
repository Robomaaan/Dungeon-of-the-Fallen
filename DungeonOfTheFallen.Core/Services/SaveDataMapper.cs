using DungeonOfTheFallen.Core.Models;
using DungeonOfTheFallen.Core.Persistence;

namespace DungeonOfTheFallen.Core.Services
{
    public static class SaveDataMapper
    {
        public static SaveData ToSaveData(GameState gameState)
        {
            return new SaveData
            {
                SaveVersion = SaveConstants.CurrentSaveVersion,
                PlayerName = gameState.Player.Name,
                PlayerPositionX = gameState.Player.PositionX,
                PlayerPositionY = gameState.Player.PositionY,
                PlayerHP = gameState.Player.HP,
                PlayerMaxHP = gameState.Player.MaxHP,
                PlayerAttack = gameState.Player.Attack,
                PlayerDefense = gameState.Player.Defense,
                PlayerXP = gameState.Player.XP,
                PlayerLevel = gameState.Player.Level,
                PlayerGold = gameState.Player.Gold,
                PotionCount = gameState.Player.Inventory.Items.Count(i => i.ItemType == ItemType.Potion),
                Enemies = gameState.Enemies
                    .Where(e => e.IsAlive)
                    .Select(e => new EnemySaveData
                    {
                        Name = e.Name,
                        EnemyType = e.EnemyType,
                        PositionX = e.PositionX,
                        PositionY = e.PositionY,
                        HP = e.HP,
                        MaxHP = e.MaxHP,
                        Attack = e.Attack,
                        Defense = e.Defense,
                        IsBoss = e.IsBoss,
                        GoldReward = e.GoldReward,
                        XpReward = e.XpReward
                    })
                    .ToList()
            };
        }

        public static void ApplyToGameState(GameState gameState, SaveData data)
        {
            gameState.Player.Name = data.PlayerName;
            gameState.Player.HP = data.PlayerHP;
            gameState.Player.MaxHP = data.PlayerMaxHP;
            gameState.Player.Attack = data.PlayerAttack;
            gameState.Player.Defense = data.PlayerDefense;
            gameState.Player.XP = data.PlayerXP;
            gameState.Player.Level = data.PlayerLevel;
            gameState.Player.Gold = data.PlayerGold;
            gameState.Player.PositionX = data.PlayerPositionX;
            gameState.Player.PositionY = data.PlayerPositionY;

            gameState.Player.Inventory.Clear();
            for (int i = 0; i < data.PotionCount; i++)
                gameState.Player.Inventory.Add(new Potion("Health Potion", 25));

            foreach (var enemy in gameState.Enemies)
            {
                var tile = gameState.Map.GetTile(enemy.PositionX, enemy.PositionY);
                if (tile != null && tile.Enemy == enemy)
                    tile.Enemy = null;
            }

            gameState.Enemies.Clear();
            foreach (var enemyData in data.Enemies)
            {
                var enemy = new Enemy(enemyData.Name, enemyData.EnemyType, enemyData.MaxHP, enemyData.Attack, enemyData.Defense, enemyData.IsBoss)
                {
                    PositionX = enemyData.PositionX,
                    PositionY = enemyData.PositionY,
                    HP = enemyData.HP,
                    GoldReward = enemyData.GoldReward,
                    XpReward = enemyData.XpReward
                };

                gameState.Enemies.Add(enemy);
                var enemyTile = gameState.Map.GetTile(enemy.PositionX, enemy.PositionY);
                if (enemyTile != null)
                    enemyTile.Enemy = enemy;
            }

            for (int y = 0; y < gameState.Map.Height; y++)
            {
                for (int x = 0; x < gameState.Map.Width; x++)
                {
                    var tile = gameState.Map.GetTile(x, y);
                    if (tile != null)
                        tile.HasPlayer = false;
                }
            }

            var playerTile = gameState.Map.GetTile(gameState.Player.PositionX, gameState.Player.PositionY);
            if (playerTile != null)
                playerTile.HasPlayer = true;
        }
    }
}
