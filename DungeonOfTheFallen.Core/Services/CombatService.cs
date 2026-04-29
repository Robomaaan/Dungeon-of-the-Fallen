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

            result.PlayerHpBefore = _gameState.Player.HP;
            result.EnemyHpBefore = enemy.HP;

            var enemyCanAct = true;
            var temporaryArmorClass = _gameState.Player.ArmorClass;
            PlayerSkill? skill = null;

            if (actionType == CombatActionType.UsePotion)
            {
                UsePotion(result);
                result.PlayerHpAfter = _gameState.Player.HP;
                result.EnemyHpAfter = enemy.HP;
                result.EnemyTurnExecuted = false;
                return result;
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
            {
                ResolveEnemyAttack(enemy, result, forcedEnemyRoll ?? RollDice(), temporaryArmorClass);
                result.EnemyTurnExecuted = true;
            }
            else
            {
                result.EnemyTurnExecuted = false;
            }

            if (!_gameState.Player.IsAlive)
            {
                result.PlayerDefeated = true;
                AddResultMessage(result, $"[DEFEAT] Du wurdest von {enemy.Name} besiegt!");
            }

            result.PlayerHpAfter = _gameState.Player.HP;
            result.EnemyHpAfter = enemy.HP;
            return result;
        }

        public int RollDice() => Dice.RollD20();

        public CombatTurnResult ForceEnemyDefeat(Enemy enemy)
        {
            var result = new CombatTurnResult();
            if (!_gameState.Player.IsAlive || !enemy.IsAlive)
                return result;

            result.PlayerHpBefore = _gameState.Player.HP;
            result.EnemyHpBefore = enemy.HP;
            HandleEnemyDefeat(enemy, result);
            result.PlayerHpAfter = _gameState.Player.HP;
            result.EnemyHpAfter = enemy.HP;
            result.EnemyTurnExecuted = false;
            return result;
        }

        private void ResolvePlayerAttack(Enemy enemy, CombatTurnResult result, int roll, PlayerSkill? skill, ref int temporaryArmorClass)
        {
            result.PlayerRoll = roll;
            result.PlayerUsedSkill = skill != null;
            result.UsedSkillName = skill?.Name;
            result.PlayerWeaponAttackBonus = _gameState.Player.Weapon.AttackBonus;
            result.PlayerSkillAttackBonus = skill?.AttackBonus ?? 0;
            result.PlayerAttackBonus = _gameState.Player.Attack + result.PlayerWeaponAttackBonus + result.PlayerSkillAttackBonus;

            if (skill != null)
            {
                var healBefore = _gameState.Player.HP;
                temporaryArmorClass += skill.DefenseBoost;
                result.DefenseBoostGranted = skill.DefenseBoost;
                if (skill.Healing > 0)
                {
                    var heal = Math.Min(skill.Healing, _gameState.Player.MaxHP - _gameState.Player.HP);
                    _gameState.Player.HP += heal;
                    result.HealingDone += heal;
                    result.PlayerHpAfter = _gameState.Player.HP;
                    AddResultMessage(result, $"[Spieler] Skill {skill.Name} aktiviert.");
                    AddResultMessage(result, $"[Spieler] Heilung: +{heal} LP ({healBefore} → {_gameState.Player.HP}).");
                }
                else
                {
                    AddResultMessage(result, $"[Spieler] Skill {skill.Name} aktiviert.");
                }

                if (skill.DefenseBoost > 0)
                    AddResultMessage(result, $"[Spieler] Rüstung für den Gegenzug: +{skill.DefenseBoost} RK.");
            }

            result.PlayerCritHit = Dice.IsNatural20(roll);
            result.PlayerCritMiss = Dice.IsNatural1(roll);
            result.PlayerTotalAttackRoll = roll == 1
                ? 1
                : roll + result.PlayerAttackBonus;

            if (result.PlayerCritMiss)
            {
                AddResultMessage(result, $"[Spieler] Angriff: W20={roll} + Angriff {_gameState.Player.Attack} + Waffe {result.PlayerWeaponAttackBonus} + Skill {result.PlayerSkillAttackBonus} = {result.PlayerTotalAttackRoll} → Patzer");
                AddResultMessage(result, $"[Spieler] {enemy.Name} weicht komplett aus.");
                return;
            }

            result.PlayerAttackHit = result.PlayerCritHit || result.PlayerTotalAttackRoll >= enemy.ArmorClass;
            if (!result.PlayerAttackHit)
            {
                AddResultMessage(result, $"[Spieler] Angriff: W20={roll} + Angriff {_gameState.Player.Attack} + Waffe {result.PlayerWeaponAttackBonus} + Skill {result.PlayerSkillAttackBonus} = {result.PlayerTotalAttackRoll} → verfehlt (RK {enemy.ArmorClass})");
                return;
            }

            var damage = _gameState.Player.Weapon.Damage.Roll(result.PlayerCritHit);
            if (skill != null)
                damage += skill.BonusDamage + (skill.BonusDamageDiceCount > 0 ? Dice.Roll(skill.BonusDamageDiceCount, skill.BonusDamageDie) : 0);

            var primaryType = skill?.BonusDamageDiceCount > 0 || skill?.BonusDamage > 0 ? skill.BonusDamageType : _gameState.Player.Weapon.Damage.DamageType;
            var adjustedDamage = enemy.DamageModifiers.Apply(primaryType, damage);
            result.PlayerDamageModifierText = enemy.DamageModifiers.Describe(primaryType);
            result.PlayerDamageDealt = adjustedDamage;
            enemy.HP = Math.Max(0, enemy.HP - adjustedDamage);
            result.EnemyHpAfter = enemy.HP;

            var critText = result.PlayerCritHit ? " Kritischer Treffer!" : string.Empty;
            var modifierText = result.PlayerDamageModifierText != "Keine Besonderheit" ? $" ({result.PlayerDamageModifierText})" : string.Empty;
            AddResultMessage(result, $"[Spieler] Angriff: W20={roll} + Angriff {_gameState.Player.Attack} + Waffe {result.PlayerWeaponAttackBonus} + Skill {result.PlayerSkillAttackBonus} = {result.PlayerTotalAttackRoll} → Treffer{critText}");
            AddResultMessage(result, $"[Spieler] Schaden: {adjustedDamage} {primaryType}-Schaden{modifierText}");
            AddResultMessage(result, $"[Gegner] LP: {result.EnemyHpBefore} → {enemy.HP}");
        }

        private void ResolveEnemyAttack(Enemy enemy, CombatTurnResult result, int roll, int targetArmorClass)
        {
            result.EnemyRoll = roll;
            result.EnemyCritHit = Dice.IsNatural20(roll);
            result.EnemyCritMiss = Dice.IsNatural1(roll);
            result.EnemyWeaponAttackBonus = enemy.Weapon.AttackBonus;
            result.EnemyAttackBonus = enemy.Attack + result.EnemyWeaponAttackBonus;
            result.EnemyTotalAttackRoll = roll == 1 ? 1 : roll + result.EnemyAttackBonus;

            if (result.EnemyCritMiss)
            {
                AddResultMessage(result, $"[Gegner] Angriff: W20={roll} + Angriff {enemy.Attack} + Waffe {result.EnemyWeaponAttackBonus} = {result.EnemyTotalAttackRoll} → Patzer");
                return;
            }

            result.EnemyAttackHit = result.EnemyCritHit || result.EnemyTotalAttackRoll >= targetArmorClass;
            if (!result.EnemyAttackHit)
            {
                AddResultMessage(result, $"[Gegner] Angriff: W20={roll} + Angriff {enemy.Attack} + Waffe {result.EnemyWeaponAttackBonus} = {result.EnemyTotalAttackRoll} → verfehlt (RK {targetArmorClass})");
                return;
            }

            var damage = enemy.Weapon.Damage.Roll(result.EnemyCritHit);
            if (_gameState.IsGodMode)
            {
                result.EnemyDamageModifierText = "Gottmodus";
                result.EnemyDamageDealt = 0;
                AddResultMessage(result, $"[Gegner] Angriff: W20={roll} + Angriff {enemy.Attack} + Waffe {result.EnemyWeaponAttackBonus} = {result.EnemyTotalAttackRoll} → Treffer");
                AddResultMessage(result, $"[Gottmodus] Schaden ignoriert.");
                AddResultMessage(result, $"[Spieler] LP: {_gameState.Player.HP} → {_gameState.Player.HP}");
                return;
            }

            var adjustedDamage = _gameState.Player.DamageModifiers.Apply(enemy.Weapon.Damage.DamageType, damage);
            result.EnemyDamageModifierText = _gameState.Player.DamageModifiers.Describe(enemy.Weapon.Damage.DamageType);
            result.EnemyDamageDealt = adjustedDamage;
            _gameState.Player.HP = Math.Max(0, _gameState.Player.HP - adjustedDamage);
            result.PlayerHpAfter = _gameState.Player.HP;

            var critText = result.EnemyCritHit ? " Kritischer Treffer!" : string.Empty;
            var modifierText = result.EnemyDamageModifierText != "Keine Besonderheit" ? $" ({result.EnemyDamageModifierText})" : string.Empty;
            AddResultMessage(result, $"[Gegner] Angriff: W20={roll} + Angriff {enemy.Attack} + Waffe {result.EnemyWeaponAttackBonus} = {result.EnemyTotalAttackRoll} → Treffer{critText}");
            AddResultMessage(result, $"[Gegner] Schaden: {adjustedDamage} {enemy.Weapon.Damage.DamageType}-Schaden{modifierText}");
            AddResultMessage(result, $"[Spieler] LP: {result.PlayerHpBefore} → {_gameState.Player.HP}");
        }

        private void UsePotion(CombatTurnResult result)
        {
            var potion = _gameState.Player.Inventory.Items.OfType<Potion>().FirstOrDefault();
            if (potion == null)
            {
                AddResultMessage(result, "[Trank] Kein Trank verfügbar.");
                return;
            }

            if (_gameState.Player.HP >= _gameState.Player.MaxHP)
            {
                result.PotionBlockedByFullHealth = true;
                AddResultMessage(result, "[Trank] LP bereits voll. Kein Trank verbraucht.");
                return;
            }

            var before = _gameState.Player.HP;
            var heal = Math.Min(potion.HealingAmount, _gameState.Player.MaxHP - _gameState.Player.HP);
            _gameState.Player.HP += heal;
            _gameState.Player.Inventory.Remove(potion);
            result.PlayerUsedPotion = true;
            result.PotionConsumed = true;
            result.HealingDone = heal;
            result.UsedPotionName = potion.Name;
            result.PlayerHpAfter = _gameState.Player.HP;
            AddResultMessage(result, $"[Trank] Spieler heilt {heal} LP.");
            AddResultMessage(result, $"[Trank] LP: {before} → {_gameState.Player.HP}");
            AddResultMessage(result, "[Trank] Kein Gegnerzug ausgelöst.");
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
                AddResultMessage(result, "[Kampf] Boss besiegt.");
            }

            AddResultMessage(result, "[Kampf] Gegner besiegt.");
            AddResultMessage(result, $"[Belohnung] +{enemy.XpReward} EP, +{enemy.GoldReward} Gold.");

            if (enemy.IsBoss)
            {
                _gameState.Player.Inventory.Add(new Potion("Boss-Trank", 50));
                AddResultMessage(result, "[Belohnung] Boss-Trank erhalten.");
            }
            else if (_gameState.EnemiesDefeatedOnFloor % 2 == 0)
            {
                _gameState.Player.Inventory.Add(new Potion("Feldtrank", 20));
                AddResultMessage(result, "[Belohnung] Ein Feldtrank fällt aus dem Kampf.");
            }

            var recovery = GameBalance.CalculatePostCombatHeal(_gameState.Player);
            if (recovery > 0)
            {
                var before = _gameState.Player.HP;
                var actualHeal = Math.Min(recovery, _gameState.Player.MaxHP - _gameState.Player.HP);
                if (actualHeal > 0)
                {
                    _gameState.Player.HP += actualHeal;
                    result.PostCombatHeal = actualHeal;
                    result.PlayerHpAfter = _gameState.Player.HP;
                    AddResultMessage(result, $"[Belohnung] Ruhepause: +{actualHeal} LP ({before} → {_gameState.Player.HP}).");
                }
            }

            _gameState.Enemies.Remove(enemy);
            var tile = _gameState.Map.GetTile(enemy.PositionX, enemy.PositionY);
            if (tile != null)
                tile.Enemy = null;

            result.PlayerLeveledUp = PlayerProgressionService.GrantXp(_gameState.Player, enemy.XpReward, result.Messages);
            result.PlayerHpAfter = _gameState.Player.HP;

            if (_gameState.BossDefeatedOnFloor && _gameState.EnemiesDefeatedOnFloor >= _gameState.FloorObjectiveTarget)
            {
                _gameState.ExitUnlocked = true;
                AddResultMessage(result, "[Phase] Ebene gesäubert. Der Ausgang ist offen.");
            }
            else
            {
                AddResultMessage(result, $"[Phase] Sichere Zwischenphase gestartet. Noch {_gameState.RemainingFloorEnemies} Gegner bis zum Ausgang.");
            }
        }

        private void AddResultMessage(CombatTurnResult result, string message)
        {
            result.Messages.Add(message);
            _gameState.AddCombatLogEntry(message);
        }
    }
}
