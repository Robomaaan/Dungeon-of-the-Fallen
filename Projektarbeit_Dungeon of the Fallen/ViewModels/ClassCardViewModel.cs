using System.Windows.Input;
using DungeonOfTheFallen.Core.Models;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    public class ClassCardViewModel : ViewModelBase
    {
        private bool _isSelected;

        public PlayerClassDefinition Definition { get; }
        public PlayerClass PlayerClass          { get; }

        public string DisplayName      => Definition.DisplayName;
        public string InitialLetter    => string.IsNullOrWhiteSpace(Definition.DisplayName)
            ? "?"
            : Definition.DisplayName.Substring(0, 1);
        public string ShortDescription => Definition.ShortDescription;
        public string PortraitImage    => Definition.PortraitImage;
        public string AccentColor      { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ICommand SelectCommand { get; }

        public ClassCardViewModel(PlayerClassDefinition def, PlayerClass playerClass,
                                  ClassSelectionViewModel parent)
        {
            Definition  = def;
            PlayerClass = playerClass;
            AccentColor = playerClass switch
            {
                PlayerClass.Warrior  => "#4F83FF",
                PlayerClass.Rogue    => "#7A4DFF",
                PlayerClass.Mage     => "#8B5CFF",
                PlayerClass.Cleric   => "#F5D76E",
                PlayerClass.Ranger   => "#4CAF50",
                PlayerClass.Assassin => "#D94B4B",
                _                   => "#FFD700"
            };

            SelectCommand = new RelayCommand(_ => parent.SelectCard(this));
        }
    }
}
