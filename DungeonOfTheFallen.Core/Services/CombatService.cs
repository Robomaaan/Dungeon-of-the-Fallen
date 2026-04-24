using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    /// <summary>
    /// Verwaltet Kampfmechaniken zwischen Spieler und Gegnern
    /// </summary>
    public class CombatService
    {
        private readonly GameState _gameState;
        public CombatService(GameState gameState)
        {
            _gameState = gameState;
        }

        /// <summary>
        /// Initiiert einen Kampf zwischen Spieler und Gegner
        /// </summary>
        public CombatTurnResult ExecuteCombat(Enemy enemy, CombatActionType actionType, int? forcedPlayerRoll = null, int? forcedEnemyRoll = null)
        {
            var result = new CombatTurnResult();

            if (!_gameState.Player.IsAlive || !enemy.IsAlive)
                return result;

            if (actionType == CombatActionType.UsePotion)
            {
                UsePotion(result);
                if (!_gameState.Player.IsAlive)
                    return result;
            }
            else
            {
                int playerRoll = forcedPlayerRoll ?? RollDice();
                result.PlayerRoll = playerRoll;

                int playerDamage = CalculateDamage(_gameState.Player.Attack, enemy.Defense, playerRoll);
                result.PlayerDamageDealt = playerDamage;
                enemy.HP -= playerDamage;
                AddResultMessage(result, $"[COMBAT] Du greifst {enemy.Name} für {playerDamage} Schaden an! (Wurf: {playerRoll})");

                if (!enemy.IsAlive)
                {
                    HandleEnemyDefeat(enemy, result);
                    return result;
                }
            }

            int enemyRoll = forcedEnemyRoll ?? RollDice();
            result.EnemyRoll = enemyRoll;
            int enemyDamage = CalculateDamage(enemy.Attack, _gameState.Player.Defense, enemyRoll);
            result.EnemyDamageDealt = enemyDamage;
            _gameState.Player.HP -= enemyDamage;
            AddResultMessage(result, $"[COMBAT] {enemy.Name} greift dich für {enemyDamage} Schaden an! (Wurf: {enemyRoll})");

            if (!_gameState.Player.IsAlive)
            {
                result.PlayerDefeated = true;
                AddResultMessage(result, $"[DEFEAT] Du wurdest von {enemy.Name} besiegt!");
            }

            return result;
        }

        /// <summary>
        /// Berechnet Schaden basierend auf Angriff und Verteidigung
        /// </summary>
        public int RollDice()
        {
            return Dice.RollD6();
        }

        public int CalculateDamage(int attack, int defense, int roll)
        {
            int baseDamage = attack - (defense / 2);
            int diceBonus = roll - 3;
            return Math.Max(1, baseDamage + diceBonus);
        }

        private void UsePotion(CombatTurnResult result)
        {
            var potion = _gameState.Player.Inventory.Items.OfType<Potion>().FirstOrDefault();
            if (potion == null)
            {
                AddResultMessage(result, "[INFO] Kein Trank im Inventar!");
                return;
            }

            int heal = Math.Min(potion.HealingAmount, _gameState.Player.MaxHP - _gameState.Player.HP);
            _gameState.Player.HP += heal;
            _gameState.Player.Inventory.Remove(potion);

            result.PlayerUsedPotion = true;
            result.HealingDone = heal;
            result.UsedPotionName = potion.Name;

            AddResultMessage(result, $"[HEAL] Trank benutzt: {potion.Name} (+{heal} HP)");
        }

        private void HandleEnemyDefeat(Enemy enemy, CombatTurnResult result)
        {
            result.EnemyDefeated = true;
            AddResultMessage(result, $"[VICTORY] {enemy.Name} besiegt!");

            var rewards = GetRewardProfile(enemy);
            _gameState.Player.XP += rewards.XpReward;
            _gameState.Player.Gold += rewards.GoldReward;
            result.XpReward = rewards.XpReward;
            result.GoldReward = rewards.GoldReward;

            AddResultMessage(result, $"[XP] +{rewards.XpReward} Erfahrung!");
            AddResultMessage(result, $"[LOOT] +{rewards.GoldReward} Gold!");

            while (_gameState.Player.XP >= _gameState.Player.Level * 200)
            {
                _gameState.Player.Level++;
                _gameState.Player.MaxHP += 10;
                _gameState.Player.HP = _gameState.Player.MaxHP;
                _gameState.Player.Attack += 2;
                _gameState.Player.Defense += 1;
                result.PlayerLeveledUp = true;
                AddResultMessage(result, $"[LEVEL UP] Du bist jetzt Level {_gameState.Player.Level}!");
            }

            if (rewards.DropsBossPotion)
            {
                _gameState.Player.Inventory.Add(new Potion("Heiltrank", 50));
                AddResultMessage(result, "[LOOT] Heiltrank gefunden!");
            }

            _gameState.Enemies.Remove(enemy);
            var tile = _gameState.Map.GetTile(enemy.PositionX, enemy.PositionY);
            if (tile != null)
                tile.Enemy = null;
        }

        private EnemyRewardProfile GetRewardProfile(Enemy enemy)
        {
            return enemy.EnemyType switch
            {
                EnemyType.Boss => new EnemyRewardProfile { GoldReward = 500, XpReward = 500, DropsBossPotion = true },
                EnemyType.Orc => new EnemyRewardProfile { GoldReward = 100, XpReward = 150 },
                _ => new EnemyRewardProfile { GoldReward = 50, XpReward = 100 }
            };
        }

        private void AddResultMessage(CombatTurnResult result, string message)
        {
            result.Messages.Add(message);
            _gameState.AddCombatLogEntry(message);
        }
    }
}
