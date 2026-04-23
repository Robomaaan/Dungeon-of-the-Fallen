using DungeonOfTheFallen.Core.Models;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        public Player Model { get; }

        public string StatusText => string.Join(
            " | ",
            Name,
            $"Lvl {Level}  HP {HP}/{MaxHP}",
            $"XP {XP}/{Level * 200}",
            $"Gold {Gold}",
            $"ATK {Attack}  DEF {Defense}",
            $"Potions {PotionCount}");

        public string Name => Model.Name;
        public int HP => Model.HP;
        public int MaxHP => Model.MaxHP;
        public int Level => Model.Level;
        public int XP => Model.XP;
        public int Gold => Model.Gold;
        public int Attack => Model.Attack;
        public int Defense => Model.Defense;
        public int PotionCount => Model.Inventory.Items.Count(i => i.ItemType == ItemType.Potion);

        public PlayerViewModel(Player player)
        {
            ArgumentNullException.ThrowIfNull(player);
            Model = player;
        }

        public void UpdateStatus()
        {
            OnPropertyChanged(
                nameof(StatusText),
                nameof(Name),
                nameof(HP),
                nameof(MaxHP),
                nameof(Level),
                nameof(XP),
                nameof(Gold),
                nameof(Attack),
                nameof(Defense),
                nameof(PotionCount));
        }
    }
}
