using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public class TurnManager
    {
        private readonly GameState _gameState;
        private readonly Random _random = new();

        public TurnManager(GameState gameState) => _gameState = gameState;

        public bool MovePlayer(int newX, int newY)
        {
            if (newX < 0 || newX >= _gameState.Map.Width || newY < 0 || newY >= _gameState.Map.Height)
                return false;

            var targetTile = _gameState.Map.GetTile(newX, newY);
            if (targetTile == null)
                return false;

            if (targetTile.TileType == TileType.LockedDoor)
            {
                if (string.IsNullOrWhiteSpace(targetTile.DoorKeyId) || !_gameState.HasKey(targetTile.DoorKeyId))
                {
                    _gameState.AddCombatLogEntry($"[DOOR] Verschlossen. {targetTile.HintText ?? "Finde den passenden Schlüssel."}");
                    return false;
                }

                targetTile.TileType = TileType.Floor;
                _gameState.AddCombatLogEntry($"[DOOR] Du öffnest die Tür mit dem Schlüssel '{targetTile.DoorKeyId}'.");
            }

            if (!targetTile.IsWalkable || (targetTile.Enemy != null && targetTile.Enemy.IsAlive))
                return false;

            var oldTile = _gameState.Map.GetTile(_gameState.Player.PositionX, _gameState.Player.PositionY);
            if (oldTile != null) oldTile.HasPlayer = false;

            _gameState.Player.PositionX = newX;
            _gameState.Player.PositionY = newY;
            targetTile.HasPlayer = true;

            PickUpItem(targetTile);
            ApplyTileEffect(targetTile);
            ApplyNpcInteraction(targetTile);
            ApplyPuzzle(targetTile);

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
            var playerX = _gameState.Player.PositionX;
            var playerY = _gameState.Player.PositionY;
            var dist = Math.Abs(enemy.PositionX - playerX) + Math.Abs(enemy.PositionY - playerY);

            if (dist <= 1)
            {
                var roll = Dice.RollD20();
                if (roll == 1)
                {
                    _gameState.AddCombatLogEntry($"[ANGRIFF] {enemy.Name} stolpert beim Gelegenheitsangriff.");
                    return;
                }

                var hit = roll == 20 || roll + enemy.Attack + enemy.Weapon.AttackBonus >= _gameState.Player.ArmorClass;
                if (!hit)
                {
                    _gameState.AddCombatLogEntry($"[ANGRIFF] {enemy.Name} verfehlt dich im Gedränge.");
                    return;
                }

                var damage = enemy.Weapon.Damage.Roll(roll == 20);
                damage = _gameState.Player.DamageModifiers.Apply(enemy.Weapon.Damage.DamageType, damage);
                _gameState.Player.HP -= damage;
                _gameState.AddCombatLogEntry($"[ANGRIFF] {enemy.Name} trifft dich für {damage} Schaden!");
                return;
            }

            var (dx, dy) = GetMovementStep(enemy, playerX, playerY, dist);
            if (dx == 0 && dy == 0)
                return;

            var newX = enemy.PositionX + dx;
            var newY = enemy.PositionY + dy;
            var tile = _gameState.Map.GetTile(newX, newY);
            if (tile == null || !tile.IsWalkable || tile.Enemy != null || tile.HasPlayer || tile.TileType == TileType.LockedDoor)
                return;

            var oldTile = _gameState.Map.GetTile(enemy.PositionX, enemy.PositionY);
            if (oldTile != null && oldTile.Enemy == enemy) oldTile.Enemy = null;

            enemy.PositionX = newX;
            enemy.PositionY = newY;
            tile.Enemy = enemy;
        }

        private (int dx, int dy) GetMovementStep(Enemy enemy, int playerX, int playerY, int dist)
        {
            if (dist > 9)
            {
                if (_random.Next(4) != 0) return (0, 0);
                var dirs = new[] { (0, 1), (0, -1), (1, 0), (-1, 0) };
                return dirs[_random.Next(dirs.Length)];
            }

            var diffX = playerX - enemy.PositionX;
            var diffY = playerY - enemy.PositionY;
            if (enemy.Role == EnemyRole.Scout)
                return Math.Abs(diffY) >= Math.Abs(diffX) ? (0, Math.Sign(diffY)) : (Math.Sign(diffX), 0);

            return Math.Abs(diffX) >= Math.Abs(diffY) ? (Math.Sign(diffX), 0) : (0, Math.Sign(diffY));
        }

        private void PickUpItem(Tile tile)
        {
            if (tile.Item == null) return;
            var item = tile.Item;
            tile.Item = null;

            switch (item)
            {
                case Potion potion:
                    _gameState.Player.Inventory.Add(potion);
                    _gameState.AddCombatLogEntry($"[ITEM] {potion.Name} aufgehoben (+{potion.HealingAmount} HP Heilung)");
                    break;
                case KeyItem key:
                    _gameState.Player.Inventory.Add(key);
                    _gameState.AddKey(key.KeyId);
                    _gameState.AddCombatLogEntry($"[KEY] {key.Name} gefunden. Passt zu '{key.KeyId}'.");
                    break;
                default:
                    if (item.ItemType == ItemType.Gold)
                    {
                        _gameState.Player.Gold += 20;
                        _gameState.AddCombatLogEntry("[LOOT] 20 Gold gefunden!");
                    }
                    break;
            }
        }

        private void ApplyNpcInteraction(Tile tile)
        {
            if (tile.Npc != null)
                NpcInteractionService.Interact(_gameState, tile.Npc);
        }

        private void ApplyPuzzle(Tile tile)
        {
            if (tile.TileType != TileType.Puzzle || string.IsNullOrWhiteSpace(tile.PuzzleId))
                return;

            var puzzle = _gameState.Puzzles.FirstOrDefault(p => p.PuzzleId == tile.PuzzleId);
            if (puzzle == null || puzzle.Solved)
                return;

            puzzle.Solved = true;
            _gameState.Player.Gold += 30 + (_gameState.CurrentFloor * 10);
            _gameState.AddCombatLogEntry($"[PUZZLE] {puzzle.Title}: {puzzle.Riddle}");
            _gameState.AddCombatLogEntry($"[PUZZLE] Lösungseintrag gefunden. {puzzle.RewardText}");
        }

        private void ApplyTileEffect(Tile tile)
        {
            switch (tile.TileType)
            {
                case TileType.HealingRoom:
                case TileType.HealingShrine:
                case TileType.HealingAltar:
                case TileType.HotSpring:
                case TileType.HealingBubble:
                case TileType.LightCircle:
                    var heal = Math.Min(18, _gameState.Player.MaxHP - _gameState.Player.HP);
                    if (heal > 0)
                    {
                        _gameState.Player.HP += heal;
                        _gameState.AddCombatLogEntry($"[HEILUNG] Der Ruhepunkt stellt {heal} HP wieder her!");
                    }
                    break;
                case TileType.Trap:
                case TileType.ThornTrap:
                case TileType.CurseTrap:
                case TileType.LavaTrap:
                case TileType.SpikeTrap:
                case TileType.DivineTrap:
                    var dmg = Math.Max(1, Dice.RollD6() + _gameState.CurrentFloor - 1);
                    _gameState.Player.HP -= dmg;
                    _gameState.AddCombatLogEntry($"[FALLE] Du hast eine Falle ausgelöst! -{dmg} HP!");
                    break;
                case TileType.KeyPedestal:
                    _gameState.AddCombatLogEntry($"[ALTAR] {tile.HintText}");
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
