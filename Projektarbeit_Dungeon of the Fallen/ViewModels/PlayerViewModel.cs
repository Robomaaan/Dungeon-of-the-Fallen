using DungeonOfTheFallen.Core.Models;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    /// <summary>
    /// ViewModel für Spieler-Status (HP, Level, XP, Gold)
    /// </summary>
    public class PlayerViewModel : ViewModelBase
    {
        private Player _model;
        private string _statusText;

        public Player Model => _model;

        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        public string Name => _model.Name;
        public int HP => _model.HP;
        public int MaxHP => _model.MaxHP;
        public int Level => _model.Level;
        public int XP => _model.XP;
        public int Gold => _model.Gold;
        public int Attack => _model.Attack;
        public int Defense => _model.Defense;

        public PlayerViewModel(Player player)
        {
            _model = player;
            _statusText = "";
            UpdateStatus();
        }

        public void UpdateStatus()
        {
            StatusText = $"{Name} | Lvl {Level} HP {HP}/{MaxHP} | XP {XP} | Gold {Gold} | ATK {Attack} DEF {Defense}";
            OnPropertyChanged(nameof(HP));
            OnPropertyChanged(nameof(MaxHP));
            OnPropertyChanged(nameof(Level));
            OnPropertyChanged(nameof(XP));
            OnPropertyChanged(nameof(Gold));
        }
    }
}
