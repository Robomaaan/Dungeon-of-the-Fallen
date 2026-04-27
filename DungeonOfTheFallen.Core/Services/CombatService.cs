using DungeonOfTheFallen.Core.Models;

namespace DungeonOfTheFallen.Core.Services
{
    public class CombatService
    {
        private readonly GameState _gameState;

        public CombatService(GameState gameState) => _gameState = gameState;

        public CombatTurnResult ExecuteCombat(Enemy enemy, CombatActionType actionType, int? forcedPlayerRoll = null, int? forcedEnemyRoll = null)
        {
            var result = new CombatTurnResult();
            if (!_gameState.Player.IsAlive || !enemy.IsAlive)
                return result;

            var enemyCanAct = true;
            var temporaryArmorClass = _gameState.Player.ArmorClass;
            PlayerSkill? skill = null;

            if (actionType == CombatActionType.UsePotion)
            {
                UsePotion(result);
            }
            else
            {
                skill = actionType == CombatActionType.UseSkill ? _gameState.Player.ClassSkill : null;
                ResolvePlayerAttack(enemy, result, forcedPlayerRoll ?? RollDice(), skill, ref temporaryArmorClass);
                if (!enemy.IsAlive)
                {
                    HandleEnemyDefeat(enemy, result);
                    enemyCanAct = false;
                }
            }

            if (enemyCanAct && _gameState.Player.IsAlive)
                ResolveEnemyAttack(enemy, result, forcedEnemyRoll ?? RollDice(), temporaryArmorClass);

            if (!_gameState.Player.IsAlive)
            {
                result.PlayerDefeated = true;
                AddResultMessage(result, $"[DEFEAT] Du wurdest von {enemy.Name} besiegt!");
            }

            return result;
        }

        public int RollDice() => Dice.RollD20();

        private void ResolvePlayerAttack(Enemy enemy, CombatTurnResult result, int roll, PlayerSkill? skill, ref int temporaryArmorClass)
        {
            result.PlayerRoll = roll;
            result.PlayerUsedSkill = skill != null;
            result.UsedSkillName = skill?.Name;

            if (skill != null)
            {
                temporaryArmorClass += skill.DefenseBoost;
                result.DefenseBoostGranted = skill.DefenseBoost;
                if (skill.Healing > 0)
                {
                    var heal = Math.Min(skill.Healing, _gameState.Player.MaxHP - _gameState.Player.HP);
                    _gameState.Player.HP += heal;
                    result.HealingDone += heal;
                }
                AddResultMessage(result, $"[SKILL] {skill.Name} aktiviert! {skill.Description}");
            }

            result.PlayerCritHit = Dice.IsNatural20(roll);
            result.PlayerCritMiss = Dice.IsNatural1(roll);
            result.PlayerTotalAttackRoll = roll == 1
                ? 1
                : roll + _gameState.Player.Attack + _gameState.Player.Weapon.AttackBonus + (skill?.AttackBonus ?? 0);

            if (result.PlayerCritMiss)
            {
                AddResultMessage(result, $"[COMBAT] Kritischer Fehlschlag! {enemy.Name} weicht komplett aus.");
                return;
            }

            result.PlayerAttackHit = result.PlayerCritHit || result.PlayerTotalAttackRoll >= enemy.ArmorClass;
            if (!result.PlayerAttackHit)
            {
                AddResultMessage(result, $"[COMBAT] Angriff verfehlt! Wurf {result.PlayerTotalAttackRoll} gegen AC {enemy.ArmorClass}.");
                return;
            }

            var damage = _gameState.Player.Weapon.Damage.Roll(result.PlayerCritHit);
            if (skill != null)
                damage += skill.BonusDamage + (skill.BonusDamageDiceCount > 0 ? Dice.Roll(skill.BonusDamageDiceCount, skill.BonusDamageDie) : 0);

            var primaryType = skill?.BonusDamageDiceCount > 0 || skill?.BonusDamage > 0 ? skill.BonusDamageType : _gameState.Player.Weapon.Damage.DamageType;
            var adjustedDamage = enemy.DamageModifiers.Apply(primaryType, damage);
            result.PlayerDamageModifierText = enemy.DamageModifiers.Describe(primaryType);
            result.PlayerDamageDealt = adjustedDamage;
            enemy.HP -= adjustedDamage;

            var critText = result.PlayerCritHit ? " Kritischer Treffer!" : string.Empty;
            var modifierText = result.PlayerDamageModifierText != "Normal" ? $" ({result.PlayerDamageModifierText})" : string.Empty;
            AddResultMessage(result, $"[COMBAT] Du triffst {enemy.Name} mit {_gameState.Player.Weapon.Name} für {adjustedDamage} {primaryType}-Schaden.{critText}{modifierText}");
        }

        private void ResolveEnemyAttack(Enemy enemy, CombatTurnResult result, int roll, int targetArmorClass)
        {
            result.EnemyRoll = roll;
            result.EnemyCritHit = Dice.IsNatural20(roll);
            result.EnemyCritMiss = Dice.IsNatural1(roll);
            result.EnemyTotalAttackRoll = roll == 1 ? 1 : roll + enemy.Attack + enemy.Weapon.AttackBonus;

            if (result.EnemyCritMiss)
            {
                AddResultMessage(result, $"[COMBAT] {enemy.Name} patzt beim Angriff.");
                return;
            }

            result.EnemyAttackHit = result.EnemyCritHit || result.EnemyTotalAttackRoll >= targetArmorClass;
            if (!result.EnemyAttackHit)
            {
                AddResultMessage(result, $"[COMBAT] {enemy.Name} verfehlt dich. Wurf {result.EnemyTotalAttackRoll} gegen AC {targetArmorClass}.");
                return;
            }

            var damage = enemy.Weapon.Damage.Roll(result.EnemyCritHit);
            var adjustedDamage = _gameState.Player.DamageModifiers.Apply(enemy.Weapon.Damage.DamageType, damage);
            result.EnemyDamageModifierText = _gameState.Player.DamageModifiers.Describe(enemy.Weapon.Damage.DamageType);
            result.EnemyDamageDealt = adjustedDamage;
            _gameState.Player.HP -= adjustedDamage;

            var critText = result.EnemyCritHit ? " Kritischer Treffer!" : string.Empty;
            var modifierText = result.EnemyDamageModifierText != "Normal" ? $" ({result.EnemyDamageModifierText})" : string.Empty;
            AddResultMessage(result, $"[COMBAT] {enemy.Name} trifft dich für {adjustedDamage} {enemy.Weapon.Damage.DamageType}-Schaden.{critText}{modifierText}");
        }

        private void UsePotion(CombatTurnResult result)
        {
            var potion = _gameState.Player.Inventory.Items.OfType<Potion>().FirstOrDefault();
            if (potion == null)
            {
                AddResultMessage(result, "[INFO] Kein Trank im Inventar!");
                return;
            }

            var heal = Math.Min(potion.HealingAmount, _gameState.Player.MaxHP - _gameState.Player.HP);
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
            result.XpReward = enemy.XpReward;
            result.GoldReward = enemy.GoldReward;
            _gameState.Player.Gold += enemy.GoldReward;
            _gameState.EnemiesDefeatedOnFloor++;
            if (enemy.IsBoss)
            {
                _gameState.BossDefeatedOnFloor = true;
                AddResultMessage(result, "[OBJECTIVE] Der Boss der Ebene ist gefallen!");
            }

            AddResultMessage(result, $"[VICTORY] {enemy.Name} besiegt!");
            AddResultMessage(result, $"[XP] +{enemy.XpReward} Erfahrung!");
            AddResultMessage(result, $"[LOOT] +{enemy.GoldReward} Gold!");
            result.PlayerLeveledUp = PlayerProgressionService.GrantXp(_gameState.Player, enemy.XpReward, result.Messages);

            if (enemy.IsBoss)
            {
                _gameState.Player.Inventory.Add(new Potion("Boss Draught", 50));
                AddResultMessage(result, "[LOOT] Boss-Trank erhalten.");
            }
            else if (_gameState.EnemiesDefeatedOnFloor % 2 == 0)
            {
                _gameState.Player.Inventory.Add(new Potion("Field Potion", 20));
                AddResultMessage(result, "[LOOT] Ein Feldtrank fällt aus dem Kampf.");
            }

            _gameState.Enemies.Remove(enemy);
            var tile = _gameState.Map.GetTile(enemy.PositionX, enemy.PositionY);
            if (tile != null)
                tile.Enemy = null;

            if (_gameState.BossDefeatedOnFloor && _gameState.EnemiesDefeatedOnFloor >= _gameState.FloorObjectiveTarget)
            {
                _gameState.ExitUnlocked = true;
                AddResultMessage(result, "[OBJECTIVE] Boss besiegt und Ebene gesäubert. Der Exit ist offen!");
            }
            else
            {
                AddResultMessage(result, $"[OBJECTIVE] Noch {_gameState.RemainingFloorEnemies} Gegner bis zum Exit.");
            }
        }

        private void AddResultMessage(CombatTurnResult result, string message)
        {
            result.Messages.Add(message);
            _gameState.AddCombatLogEntry(message);
        }
    }
}
