using System.Windows.Input;
using DungeonOfTheFallen.Core.Models;
using DungeonOfTheFallen.Core.Services;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    public enum CombatPhase
    {
        PlayerTurn,
        BothRolling,    // Spieler + Gegner gleichzeitig (Angriff / Skill)
        EnemyRolling,   // Gegner-Würfel allein (Trank-Gegenzug)
        Victory,
        Defeat
    }

    public enum WinningSide
    {
        None,
        Player,
        Enemy,
        Tie
    }

    public class CombatViewModel : ViewModelBase
    {
        private const int MaxCombatLogLines = 8;

        private readonly GameState _gameState;
        private readonly Enemy _enemy;
        private readonly CombatService _combatService;

        private CombatPhase _phase = CombatPhase.PlayerTurn;
        private int _playerDiceValue;
        private int _enemyDiceValue;
        private string _combatLog = string.Empty;
        private string _statusMessage = string.Empty;
        private string _diceResultText = string.Empty;
        private WinningSide _diceWinner = WinningSide.None;
        private bool _isDiceAnimationRunning;
        private CombatActionType _pendingAction = CombatActionType.Attack;
        private CombatTurnResult? _pendingCombatResult;

        public string PlayerName => _gameState.Player.Name;
        public int PlayerHP => _gameState.Player.HP;
        public int PlayerMaxHP => _gameState.Player.MaxHP;
        public int PlayerArmorClass => _gameState.Player.ArmorClass;
        public string PlayerWeapon => _gameState.Player.Weapon.Summary;
        public string SkillName => _gameState.Player.ClassSkill.Name;
        public string SkillDescription => _gameState.Player.ClassSkill.Description;
        public string SkillButtonText => $"✨ {SkillName}";

        public string EnemyName => _enemy.Name;
        public int EnemyHP => _enemy.HP;
        public int EnemyMaxHP => _enemy.MaxHP;
        public int EnemyArmorClass => _enemy.ArmorClass;
        public string EnemyWeapon => _enemy.Weapon.Summary;
        public EnemyType EnemyType => _enemy.EnemyType;

        public CombatPhase Phase
        {
            get => _phase;
            private set
            {
                _phase = value;
                OnPropertyChanged(nameof(Phase), nameof(IsPlayerTurn), nameof(IsGameOver), nameof(IsVictory));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsPlayerTurn => _phase == CombatPhase.PlayerTurn;
        public bool IsGameOver => _phase == CombatPhase.Victory || _phase == CombatPhase.Defeat;
        public bool IsVictory => _phase == CombatPhase.Victory;

        public int PlayerDiceValue { get => _playerDiceValue; private set => SetProperty(ref _playerDiceValue, value); }
        public int EnemyDiceValue  { get => _enemyDiceValue;  private set => SetProperty(ref _enemyDiceValue,  value); }
        public string CombatLog    { get => _combatLog;       private set => SetProperty(ref _combatLog,       value); }
        public string StatusMessage{ get => _statusMessage;   private set => SetProperty(ref _statusMessage,   value); }
        public string DiceResultText { get => _diceResultText; private set => SetProperty(ref _diceResultText, value); }

        public WinningSide DiceWinner
        {
            get => _diceWinner;
            private set
            {
                SetProperty(ref _diceWinner, value);
                OnPropertyChanged(nameof(PlayerDiceWon), nameof(EnemyDiceWon));
            }
        }
        public bool PlayerDiceWon => _diceWinner == WinningSide.Player || _diceWinner == WinningSide.Tie;
        public bool EnemyDiceWon  => _diceWinner == WinningSide.Enemy  || _diceWinner == WinningSide.Tie;

        public bool IsDiceAnimationRunning
        {
            get => _isDiceAnimationRunning;
            set => SetProperty(ref _isDiceAnimationRunning, value);
        }

        public bool HasPotions => _gameState.Player.Inventory.Items.Any(i => i.ItemType == ItemType.Potion);
        public int PotionCount => _gameState.Player.Inventory.Items.Count(i => i.ItemType == ItemType.Potion);
        public string PotionButtonText => $"🧪 TRANK ({PotionCount})";

        public ICommand AttackCommand { get; }
        public ICommand UsePotionCommand { get; }
        public ICommand UseSkillCommand { get; }

        /// <summary>Beide Würfel starten gleichzeitig (Angriff / Skill).</summary>
        public event Action<int, int>? DiceBattleStarted;

        /// <summary>Nur Gegner-Würfel (Trank-Gegenzug).</summary>
        public event Action<int, bool>? DiceRollStarted;

        public CombatViewModel(GameState gameState, Enemy enemy)
        {
            _gameState = gameState;
            _enemy = enemy;
            _combatService = new CombatService(gameState);

            AttackCommand = new RelayCommand(_ => StartPlayerAttack(CombatActionType.Attack), _ => IsPlayerTurn);
            UsePotionCommand = new RelayCommand(_ => StartPotionTurn(), _ => IsPlayerTurn && HasPotions);
            UseSkillCommand = new RelayCommand(_ => StartPlayerAttack(CombatActionType.UseSkill), _ => IsPlayerTurn);

            StatusMessage = $"Ein {_enemy.Name} blockiert den Weg! AC {_enemy.ArmorClass} · {_enemy.Weapon.Damage.Describe()}";
            AddLog($"Kampf gegen {_enemy.Name}! AC {_enemy.ArmorClass} | {_enemy.Weapon.Summary}");
        }

        private void StartPlayerAttack(CombatActionType actionType)
        {
            if (!IsPlayerTurn) return;
            _pendingAction = actionType;

            var playerRoll = _combatService.RollDice();
            var enemyRoll  = _combatService.RollDice();
            var result     = _combatService.ExecuteCombat(_enemy, actionType, playerRoll, enemyRoll);

            _pendingCombatResult = result;
            PlayerDiceValue = playerRoll;
            EnemyDiceValue  = enemyRoll;

            DiceWinner = playerRoll > enemyRoll ? WinningSide.Player
                       : playerRoll < enemyRoll ? WinningSide.Enemy
                       : WinningSide.Tie;
            DiceResultText = DiceWinner switch
            {
                WinningSide.Player => $"Spielerwürfel ({playerRoll}) schlägt Gegnerwürfel ({enemyRoll})!",
                WinningSide.Enemy  => $"Gegnerwürfel ({enemyRoll}) schlägt Spielerwürfel ({playerRoll})!",
                WinningSide.Tie    => $"Gleichstand! Beide würfeln {playerRoll}.",
                _                  => string.Empty
            };

            Phase = CombatPhase.BothRolling;
            StatusMessage = actionType == CombatActionType.UseSkill
                ? $"{SkillName} – Würfel fliegen!"
                : "Würfel fliegen!";

            System.Diagnostics.Debug.WriteLine("[DiceBattle] Start");
            System.Diagnostics.Debug.WriteLine($"[DiceBattle] PlayerRoll={playerRoll}");
            System.Diagnostics.Debug.WriteLine($"[DiceBattle] EnemyRoll={enemyRoll}");
            System.Diagnostics.Debug.WriteLine($"[DiceBattle] Result={DiceWinner}");

            DiceBattleStarted?.Invoke(playerRoll, enemyRoll);
        }

        private void StartPotionTurn()
        {
            if (!IsPlayerTurn || !HasPotions) return;

            var result = _combatService.ExecuteCombat(_enemy, CombatActionType.UsePotion, forcedEnemyRoll: _combatService.RollDice());
            ApplyCombatResult(result);

            if (result.PlayerDefeated || result.EnemyDefeated)
                return;

            EnemyDiceValue = result.EnemyRoll;
            Phase          = CombatPhase.EnemyRolling;
            StatusMessage  = $"{_enemy.Name} würfelt auf Gegenangriff...";
            DiceRollStarted?.Invoke(result.EnemyRoll, false);
        }

        /// <summary>Wird nach Abschluss der simultanen Würfelanimation aufgerufen.</summary>
        public void OnDiceBattleAnimationComplete()
        {
            System.Diagnostics.Debug.WriteLine("[DiceAnimation] Finished");
            System.Diagnostics.Debug.WriteLine("[DiceAnimation] Next state released");

            if (_pendingCombatResult != null)
            {
                ApplyCombatResult(_pendingCombatResult);
                _pendingCombatResult = null;
            }

            if (!IsGameOver)
            {
                Phase = CombatPhase.PlayerTurn;
                StatusMessage = $"Dein Zug! Würfle gegen AC {_enemy.ArmorClass}.";
                NotifyStats();
            }
        }

        public void OnEnemyDiceAnimationComplete()
        {
            if (!_gameState.Player.IsAlive)
            {
                Phase = CombatPhase.Defeat;
                StatusMessage = "Du bist gefallen...";
                AddLog("Du bist gefallen!");
                NotifyStats();
                return;
            }

            Phase = CombatPhase.PlayerTurn;
            StatusMessage = $"Dein Zug! Würfle gegen AC {_enemy.ArmorClass}.";
            NotifyStats();
        }

        private void ApplyCombatResult(CombatTurnResult result)
        {
            // Würfelvergleich: rohes d20-Ergebnis bestimmt visuellen Gewinner
            if (result.PlayerRoll > 0 && result.EnemyRoll > 0)
            {
                DiceWinner = result.PlayerRoll > result.EnemyRoll ? WinningSide.Player
                           : result.PlayerRoll < result.EnemyRoll ? WinningSide.Enemy
                           : WinningSide.Tie;
                DiceResultText = DiceWinner switch
                {
                    WinningSide.Player => $"Spielerwürfel ({result.PlayerRoll}) schlägt Gegnerwürfel ({result.EnemyRoll})!",
                    WinningSide.Enemy  => $"Gegnerwürfel ({result.EnemyRoll}) schlägt Spielerwürfel ({result.PlayerRoll})!",
                    WinningSide.Tie    => $"Gleichstand! Beide würfeln {result.PlayerRoll}.",
                    _                  => string.Empty
                };
            }
            else
            {
                DiceWinner = WinningSide.None;
                DiceResultText = string.Empty;
            }

            if (result.PlayerUsedPotion)
                AddLog($"Du benutzt {result.UsedPotionName}: +{result.HealingDone} HP!");

            if (result.PlayerUsedSkill)
            {
                AddLog($"Skill {result.UsedSkillName} ausgelöst!");
                if (result.HealingDone > 0)
                    AddLog($"Die Klassenfähigkeit heilt dich um {result.HealingDone} HP.");
                if (result.DefenseBoostGranted > 0)
                    AddLog($"AC-Bonus aktiv: +{result.DefenseBoostGranted} für den Gegenschlag.");
            }

            if (result.PlayerRoll > 0)
            {
                var attackText = result.PlayerAttackHit
                    ? $"Du würfelst {result.PlayerRoll} → Treffer ({result.PlayerTotalAttackRoll} vs AC {_enemy.ArmorClass}) für {result.PlayerDamageDealt} Schaden."
                    : $"Du würfelst {result.PlayerRoll} → verfehlt ({result.PlayerTotalAttackRoll} vs AC {_enemy.ArmorClass}).";
                if (result.PlayerCritHit) attackText += " KRIT!";
                if (result.PlayerCritMiss) attackText += " Patzer!";
                AddLog(attackText);
            }

            if (result.EnemyRoll > 0)
            {
                var enemyText = result.EnemyAttackHit
                    ? $"{_enemy.Name} würfelt {result.EnemyRoll} → Treffer ({result.EnemyTotalAttackRoll} vs AC {PlayerArmorClass}) für {result.EnemyDamageDealt} Schaden."
                    : $"{_enemy.Name} würfelt {result.EnemyRoll} → verfehlt ({result.EnemyTotalAttackRoll} vs AC {PlayerArmorClass}).";
                if (result.EnemyCritHit) enemyText += " KRIT!";
                if (result.EnemyCritMiss) enemyText += " Patzer!";
                AddLog(enemyText);
            }

            if (result.EnemyDefeated)
            {
                AddLog($"+{result.GoldReward} Gold! +{result.XpReward} XP!");
                if (result.PlayerLeveledUp)
                    AddLog($"⭐ LEVEL UP! Du bist jetzt Level {_gameState.Player.Level}!");

                Phase = CombatPhase.Victory;
                StatusMessage = $"{_enemy.Name} besiegt!";
            }
            else if (result.PlayerDefeated)
            {
                Phase = CombatPhase.Defeat;
                StatusMessage = "Du bist gefallen...";
                AddLog("Du bist gefallen!");
            }

            NotifyStats();
        }

        private void AddLog(string message)
        {
            var lines = CombatLog.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
            lines.Add(message);
            if (lines.Count > MaxCombatLogLines) lines = lines.Skip(lines.Count - MaxCombatLogLines).ToList();
            CombatLog = string.Join("\n", lines);
        }

        private void NotifyStats()
        {
            OnPropertyChanged(
                nameof(PlayerHP), nameof(EnemyHP),
                nameof(HasPotions), nameof(PotionCount), nameof(PotionButtonText),
                nameof(SkillName), nameof(SkillDescription), nameof(SkillButtonText),
                nameof(PlayerArmorClass), nameof(EnemyArmorClass),
                nameof(PlayerWeapon), nameof(EnemyWeapon),
                nameof(PlayerDiceValue), nameof(EnemyDiceValue),
                nameof(DiceResultText), nameof(DiceWinner), nameof(PlayerDiceWon), nameof(EnemyDiceWon));
        }
    }
}
