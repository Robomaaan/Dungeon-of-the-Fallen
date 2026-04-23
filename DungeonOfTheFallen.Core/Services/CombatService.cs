using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    /// <summary>
    /// Verwaltet Kampfmechaniken zwischen Spieler und Gegnern
    /// </summary>
    public class CombatService
    {
        private GameState _gameState;
        private Random _random = new Random();

        public CombatService(GameState gameState)
        {
            _gameState = gameState;
        }

        /// <summary>
        /// Initiiert einen Kampf zwischen Spieler und Gegner
        /// </summary>
        public void ExecuteCombat(Enemy enemy)
        {
            if (!_gameState.Player.IsAlive || !enemy.IsAlive)
                return;

            // Spieler greift an
            int playerDamage = CalculateDamage(_gameState.Player.Attack, enemy.Defense);
            enemy.HP -= playerDamage;
            _gameState.AddCombatLogEntry($"[COMBAT] You attack {enemy.Name} for {playerDamage} damage!");

            if (!enemy.IsAlive)
            {
                _gameState.AddCombatLogEntry($"[VICTORY] {enemy.Name} defeated!");
                OnEnemyDefeated(enemy);
                return;
            }

            // Gegner greift zurück
            int enemyDamage = CalculateDamage(enemy.Attack, _gameState.Player.Defense);
            _gameState.Player.HP -= enemyDamage;
            _gameState.AddCombatLogEntry($"[COMBAT] {enemy.Name} attacks you for {enemyDamage} damage!");

            if (!_gameState.Player.IsAlive)
            {
                _gameState.AddCombatLogEntry($"[DEFEAT] You have been defeated by {enemy.Name}!");
            }
        }

        /// <summary>
        /// Berechnet Schaden basierend auf Angriff und Verteidigung
        /// </summary>
        private int CalculateDamage(int attack, int defense)
        {
            int baseDamage = attack - (defense / 2);
            int variance = _random.Next(-2, 3); // Zufälligkeit
            return Math.Max(1, baseDamage + variance);
        }

        /// <summary>
        /// Wird aufgerufen wenn Gegner besiegt ist
        /// </summary>
        private void OnEnemyDefeated(Enemy enemy)
        {
            // XP geben
            int xpReward = enemy.EnemyType == EnemyType.Boss ? 500 : (enemy.EnemyType == EnemyType.Orc ? 150 : 100);
            _gameState.Player.XP += xpReward;
            _gameState.AddCombatLogEntry($"[XP] +{xpReward} experience!");

            // Level aufsteigen?
            while (_gameState.Player.XP >= _gameState.Player.Level * 200)
            {
                _gameState.Player.Level++;
                _gameState.Player.MaxHP += 10;
                _gameState.Player.HP = _gameState.Player.MaxHP;
                _gameState.Player.Attack += 2;
                _gameState.Player.Defense += 1;
                _gameState.AddCombatLogEntry($"[LEVEL UP] You are now level {_gameState.Player.Level}!");
            }

            // Loot droppen
            DropLoot(enemy);

            // Gegner von der Map entfernen
            _gameState.Enemies.Remove(enemy);
            var tile = _gameState.Map.GetTile(enemy.PositionX, enemy.PositionY);
            if (tile != null)
                tile.Enemy = null;
        }

        /// <summary>
        /// Gegner droppt Loot
        /// </summary>
        private void DropLoot(Enemy enemy)
        {
            int goldReward = enemy.EnemyType == EnemyType.Boss ? 500 : (enemy.EnemyType == EnemyType.Orc ? 100 : 50);
            _gameState.Player.Gold += goldReward;
            _gameState.AddCombatLogEntry($"[LOOT] +{goldReward} gold!");

            // Seltene Tränke vom Boss
            if (enemy.EnemyType == EnemyType.Boss)
            {
                _gameState.Player.Inventory.Add(new Potion("Health Potion", 50));
                _gameState.AddCombatLogEntry($"[LOOT] Found a Health Potion!");
            }
        }
    }
}
