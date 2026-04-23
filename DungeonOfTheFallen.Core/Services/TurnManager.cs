using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public class TurnManager
    {
        private readonly GameState _gameState;
        private readonly Random _random = new Random();

        public TurnManager(GameState gameState)
        {
            _gameState = gameState;
        }

        /// <summary>
        /// Bewegt den Spieler. Kein Kampf hier — Kampf wird in der UI gehandhabt.
        /// </summary>
        public bool MovePlayer(int newX, int newY)
        {
            if (newX < 0 || newX >= _gameState.Map.Width || newY < 0 || newY >= _gameState.Map.Height)
                return false;

            var targetTile = _gameState.Map.GetTile(newX, newY);
            if (targetTile == null || !targetTile.IsWalkable)
                return false;

            // Feind auf Ziel-Tile blockiert Bewegung (Kampf läuft über UI)
            if (targetTile.Enemy != null && targetTile.Enemy.IsAlive)
                return false;

            var oldTile = _gameState.Map.GetTile(_gameState.Player.PositionX, _gameState.Player.PositionY);
            if (oldTile != null) oldTile.HasPlayer = false;

            _gameState.Player.PositionX = newX;
            _gameState.Player.PositionY = newY;
            targetTile.HasPlayer = true;

            PickUpItem(targetTile);
            ApplyTileEffect(targetTile);

            if (_gameState.Player.IsAlive)
                ExecuteEnemyTurns();

            return true;
        }

        private void ExecuteEnemyTurns()
        {
            foreach (var enemy in _gameState.Enemies.ToList())
            {
                if (!enemy.IsAlive) continue;
                MoveEnemy(enemy);
            }
        }

        private void MoveEnemy(Enemy enemy)
        {
            int playerX = _gameState.Player.PositionX;
            int playerY = _gameState.Player.PositionY;
            int dist = Math.Abs(enemy.PositionX - playerX) + Math.Abs(enemy.PositionY - playerY);

            // Direkt benachbart: Bump-Angriff (ohne Würfel)
            if (dist <= 1)
            {
                int dmg = Math.Max(1, enemy.Attack - _gameState.Player.Defense / 2 + _random.Next(-1, 2));
                _gameState.Player.HP -= dmg;
                _gameState.AddCombatLogEntry($"[ANGRIFF] {enemy.Name} schlägt dich für {dmg} Schaden!");
                return;
            }

            int dx = 0, dy = 0;

            if (dist <= 9)
            {
                // Spieler jagen
                int diffX = playerX - enemy.PositionX;
                int diffY = playerY - enemy.PositionY;

                if (Math.Abs(diffX) >= Math.Abs(diffY))
                    dx = Math.Sign(diffX);
                else
                    dy = Math.Sign(diffY);

                var preferred = _gameState.Map.GetTile(enemy.PositionX + dx, enemy.PositionY + dy);
                if (preferred == null || !preferred.IsWalkable || preferred.Enemy != null)
                {
                    dx = Math.Abs(diffX) >= Math.Abs(diffY) ? 0 : Math.Sign(diffX);
                    dy = Math.Abs(diffX) >= Math.Abs(diffY) ? Math.Sign(diffY) : 0;
                }
            }
            else
            {
                if (_random.Next(4) != 0) return;
                var dirs = new[] { (0, 1), (0, -1), (1, 0), (-1, 0) };
                (dx, dy) = dirs[_random.Next(dirs.Length)];
            }

            int newX = enemy.PositionX + dx;
            int newY = enemy.PositionY + dy;

            if (newX < 0 || newX >= _gameState.Map.Width || newY < 0 || newY >= _gameState.Map.Height)
                return;

            var tile = _gameState.Map.GetTile(newX, newY);
            if (tile == null || !tile.IsWalkable || tile.Enemy != null || tile.HasPlayer)
                return;

            var oldTile = _gameState.Map.GetTile(enemy.PositionX, enemy.PositionY);
            if (oldTile != null && oldTile.Enemy == enemy) oldTile.Enemy = null;

            enemy.PositionX = newX;
            enemy.PositionY = newY;
            tile.Enemy = enemy;
        }

        private void PickUpItem(Tile tile)
        {
            if (tile.Item == null) return;
            var item = tile.Item;
            tile.Item = null;

            if (item is Potion potion)
            {
                _gameState.Player.Inventory.Add(potion);
                _gameState.AddCombatLogEntry($"[ITEM] {potion.Name} aufgehoben (+{potion.HealingAmount} HP Heilung)");
            }
            else if (item.ItemType == ItemType.Gold)
            {
                _gameState.Player.Gold += 20;
                _gameState.AddCombatLogEntry("[LOOT] 20 Gold gefunden!");
            }
        }

        private void ApplyTileEffect(Tile tile)
        {
            switch (tile.TileType)
            {
                case TileType.HealingRoom:
                    int heal = Math.Min(20, _gameState.Player.MaxHP - _gameState.Player.HP);
                    if (heal > 0)
                    {
                        _gameState.Player.HP += heal;
                        _gameState.AddCombatLogEntry($"[HEILUNG] Der Heilungsraum stellt {heal} HP wieder her!");
                    }
                    break;
                case TileType.Trap:
                    int dmg = Math.Max(1, 10 - _gameState.Player.Defense / 2);
                    _gameState.Player.HP -= dmg;
                    _gameState.AddCombatLogEntry($"[FALLE] Du hast eine Falle ausgelöst! -{dmg} HP!");
                    break;
            }
        }

        public bool CheckExitReached()
        {
            var tile = _gameState.Map.GetTile(_gameState.Player.PositionX, _gameState.Player.PositionY);
            return tile?.TileType == TileType.Exit;
        }
    }
}
