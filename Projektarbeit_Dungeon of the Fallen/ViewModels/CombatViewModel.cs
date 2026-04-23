using System.Windows.Input;
using DungeonOfTheFallen.Core.Models;

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
        private readonly GameState _gameState;
        private readonly Enemy _enemy;
        private readonly Random _random = new Random();

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
            int roll = _random.Next(1, 7);
            PlayerDiceValue = roll;
            DiceRollStarted?.Invoke(roll, true);
        }

        private void StartPotionTurn()
        {
            if (!IsPlayerTurn || !HasPotions) return;

            var potion = _gameState.Player.Inventory.Items.OfType<Potion>().First();
            int heal = Math.Min(potion.HealingAmount, _gameState.Player.MaxHP - _gameState.Player.HP);
            _gameState.Player.HP += heal;
            _gameState.Player.Inventory.Remove(potion);
            AddLog($"Du benutzt {potion.Name}: +{heal} HP!");
            _gameState.AddCombatLogEntry($"[HEAL] Trank benutzt: +{heal} HP");
            NotifyStats();

            // Enemy still gets a turn
            Phase = CombatPhase.EnemyRolling;
            StatusMessage = $"{_enemy.Name} würfelt...";
            int roll = _random.Next(1, 7);
            EnemyDiceValue = roll;
            DiceRollStarted?.Invoke(roll, false);
        }

        // === Called by CombatWindow after animation completes ===

        public void OnPlayerDiceAnimationComplete()
        {
            int baseDmg = Math.Max(1, _gameState.Player.Attack - _enemy.Defense / 2);
            int diceBonus = PlayerDiceValue - 3; // -2 to +3
            int damage = Math.Max(1, baseDmg + diceBonus);

            _enemy.HP -= damage;
            AddLog($"Du würfelst {PlayerDiceValue} → {damage} Schaden an {_enemy.Name}! (HP {Math.Max(0, _enemy.HP)}/{_enemy.MaxHP})");
            _gameState.AddCombatLogEntry($"[COMBAT] Du greifst {_enemy.Name} für {damage} an (Wurf: {PlayerDiceValue})");
            NotifyStats();

            if (!_enemy.IsAlive)
            {
                OnEnemyDefeated();
                return;
            }

            Phase = CombatPhase.EnemyRolling;
            StatusMessage = $"{_enemy.Name} würfelt zurück...";
            int roll = _random.Next(1, 7);
            EnemyDiceValue = roll;
            DiceRollStarted?.Invoke(roll, false);
        }

        public void OnEnemyDiceAnimationComplete()
        {
            int baseDmg = Math.Max(1, _enemy.Attack - _gameState.Player.Defense / 2);
            int diceBonus = EnemyDiceValue - 3;
            int damage = Math.Max(1, baseDmg + diceBonus);

            _gameState.Player.HP -= damage;
            AddLog($"{_enemy.Name} würfelt {EnemyDiceValue} → {damage} Schaden an dir! (HP {Math.Max(0, _gameState.Player.HP)}/{_gameState.Player.MaxHP})");
            _gameState.AddCombatLogEntry($"[COMBAT] {_enemy.Name} greift für {damage} an (Wurf: {EnemyDiceValue})");
            NotifyStats();

            if (!_gameState.Player.IsAlive)
            {
                _gameState.AddCombatLogEntry($"[DEFEAT] Du wurdest von {_enemy.Name} besiegt!");
                Phase = CombatPhase.Defeat;
                StatusMessage = "Du bist gefallen...";
                AddLog("Du bist gefallen!");
                return;
            }

            Phase = CombatPhase.PlayerTurn;
            StatusMessage = "Dein Zug!";
        }

        private void OnEnemyDefeated()
        {
            int xpReward = _enemy.EnemyType == EnemyType.Boss ? 500 : (_enemy.EnemyType == EnemyType.Orc ? 150 : 100);
            _gameState.Player.XP += xpReward;
            _gameState.AddCombatLogEntry($"[XP] +{xpReward} Erfahrung!");

            while (_gameState.Player.XP >= _gameState.Player.Level * 200)
            {
                _gameState.Player.Level++;
                _gameState.Player.MaxHP += 10;
                _gameState.Player.HP = _gameState.Player.MaxHP;
                _gameState.Player.Attack += 2;
                _gameState.Player.Defense += 1;
                _gameState.AddCombatLogEntry($"[LEVEL UP] Level {_gameState.Player.Level}!");
                AddLog($"⭐ LEVEL UP! Du bist jetzt Level {_gameState.Player.Level}!");
            }

            int goldReward = _enemy.EnemyType == EnemyType.Boss ? 500 : (_enemy.EnemyType == EnemyType.Orc ? 100 : 50);
            _gameState.Player.Gold += goldReward;
            _gameState.AddCombatLogEntry($"[LOOT] +{goldReward} Gold!");
            AddLog($"+{goldReward} Gold! +{xpReward} XP!");

            if (_enemy.EnemyType == EnemyType.Boss)
            {
                _gameState.Player.Inventory.Add(new Potion("Heiltrank", 50));
                _gameState.AddCombatLogEntry("[LOOT] Heiltrank gefunden!");
                AddLog("Heiltrank erbeutet!");
            }

            _gameState.Enemies.Remove(_enemy);
            var tile = _gameState.Map.GetTile(_enemy.PositionX, _enemy.PositionY);
            if (tile != null) tile.Enemy = null;

            Phase = CombatPhase.Victory;
            StatusMessage = $"{_enemy.Name} besiegt!";
        }

        private void AddLog(string message)
        {
            var lines = CombatLog.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
            lines.Add(message);
            if (lines.Count > 7) lines = lines.Skip(lines.Count - 7).ToList();
            CombatLog = string.Join("\n", lines);
        }

        private void NotifyStats()
        {
            OnPropertyChanged(nameof(PlayerHP), nameof(EnemyHP), nameof(HasPotions), nameof(PotionCount), nameof(PotionButtonText));
        }
    }
}
