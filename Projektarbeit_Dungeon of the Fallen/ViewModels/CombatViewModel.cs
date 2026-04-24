using System.Windows.Input;
using DungeonOfTheFallen.Core.Models;
using DungeonOfTheFallen.Core.Services;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    public enum CombatPhase
    {
        PlayerTurn,
        PlayerRolling,
        EnemyRolling,
        Victory,
        Defeat
    }

    public class CombatViewModel : ViewModelBase
    {
        private const int MaxCombatLogLines = 7;

        private readonly GameState _gameState;
        private readonly Enemy _enemy;
        private readonly CombatService _combatService;

        private CombatPhase _phase = CombatPhase.PlayerTurn;
        private int _playerDiceValue;
        private int _enemyDiceValue;
        private string _combatLog = string.Empty;
        private string _statusMessage = string.Empty;

        public string PlayerName => _gameState.Player.Name;
        public int PlayerHP => _gameState.Player.HP;
        public int PlayerMaxHP => _gameState.Player.MaxHP;

        public string EnemyName => _enemy.Name;
        public int EnemyHP => _enemy.HP;
        public int EnemyMaxHP => _enemy.MaxHP;
        public EnemyType EnemyType => _enemy.EnemyType;

        public CombatPhase Phase
        {
            get => _phase;
            private set
            {
                _phase = value;
                OnPropertyChanged(nameof(Phase), nameof(IsPlayerTurn), nameof(IsGameOver), nameof(IsVictory));
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsPlayerTurn => _phase == CombatPhase.PlayerTurn;
        public bool IsGameOver => _phase == CombatPhase.Victory || _phase == CombatPhase.Defeat;
        public bool IsVictory => _phase == CombatPhase.Victory;

        public int PlayerDiceValue
        {
            get => _playerDiceValue;
            private set => SetProperty(ref _playerDiceValue, value);
        }

        public int EnemyDiceValue
        {
            get => _enemyDiceValue;
            private set => SetProperty(ref _enemyDiceValue, value);
        }

        public string CombatLog
        {
            get => _combatLog;
            private set => SetProperty(ref _combatLog, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool HasPotions => _gameState.Player.Inventory.Items.Any(i => i.ItemType == ItemType.Potion);
        public int PotionCount => _gameState.Player.Inventory.Items.Count(i => i.ItemType == ItemType.Potion);
        public string PotionButtonText => $"🧪 TRANK ({PotionCount})";

        public ICommand AttackCommand { get; }
        public ICommand UsePotionCommand { get; }

        // View subscribes: (diceValue, isPlayerRoll)
        public event Action<int, bool>? DiceRollStarted;

        public CombatViewModel(GameState gameState, Enemy enemy)
        {
            _gameState = gameState;
            _enemy = enemy;
            _combatService = new CombatService(gameState);

            AttackCommand = new RelayCommand(_ => StartPlayerAttack(), _ => IsPlayerTurn);
            UsePotionCommand = new RelayCommand(_ => StartPotionTurn(), _ => IsPlayerTurn && HasPotions);

            StatusMessage = $"Ein {_enemy.Name} greift an!";
            AddLog($"Kampf gegen {_enemy.Name}! (HP {_enemy.HP}/{_enemy.MaxHP})");
        }

        // === Player actions ===

        private void StartPlayerAttack()
        {
            if (!IsPlayerTurn) return;
            Phase = CombatPhase.PlayerRolling;
            StatusMessage = "Du würfelst...";
            int roll = _combatService.RollDice();
            PlayerDiceValue = roll;
            DiceRollStarted?.Invoke(roll, true);
        }

        private void StartPotionTurn()
        {
            if (!IsPlayerTurn || !HasPotions) return;

            var result = _combatService.ExecuteCombat(_enemy, CombatActionType.UsePotion, forcedEnemyRoll: _combatService.RollDice());
            ApplyCombatResult(result);

            if (result.PlayerDefeated || result.EnemyDefeated)
                return;

            EnemyDiceValue = result.EnemyRoll;
            Phase = CombatPhase.EnemyRolling;
            StatusMessage = $"{_enemy.Name} würfelt...";
            DiceRollStarted?.Invoke(result.EnemyRoll, false);
        }

        // === Called by CombatWindow after animation completes ===

        public void OnPlayerDiceAnimationComplete()
        {
            var result = _combatService.ExecuteCombat(_enemy, CombatActionType.Attack, PlayerDiceValue);

            AddLog($"Du würfelst {PlayerDiceValue} → {result.PlayerDamageDealt} Schaden an {_enemy.Name}! (HP {Math.Max(0, _enemy.HP)}/{_enemy.MaxHP})");
            ApplyCombatResult(result);

            if (result.PlayerDefeated || result.EnemyDefeated)
                return;

            EnemyDiceValue = result.EnemyRoll;
            Phase = CombatPhase.EnemyRolling;
            StatusMessage = $"{_enemy.Name} würfelt zurück...";
            DiceRollStarted?.Invoke(result.EnemyRoll, false);
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
            StatusMessage = "Dein Zug!";
            NotifyStats();
        }

        private void ApplyCombatResult(CombatTurnResult result)
        {
            if (result.PlayerUsedPotion)
            {
                AddLog($"Du benutzt {result.UsedPotionName}: +{result.HealingDone} HP!");
            }

            if (result.EnemyDamageDealt > 0)
            {
                AddLog($"{_enemy.Name} würfelt {result.EnemyRoll} → {result.EnemyDamageDealt} Schaden an dir! (HP {Math.Max(0, _gameState.Player.HP)}/{_gameState.Player.MaxHP})");
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
            OnPropertyChanged(nameof(PlayerHP), nameof(EnemyHP), nameof(HasPotions), nameof(PotionCount), nameof(PotionButtonText));
        }
    }
}
