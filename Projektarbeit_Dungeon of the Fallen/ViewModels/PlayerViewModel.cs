using DungeonOfTheFallen.Core.Models;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        public Player Model { get; }

        public string StatusText => string.Join(
            " | ",
            Name,
            PlayerClassDisplay,
            $"Lvl {Level}  HP {HP}/{MaxHP}",
            $"XP {XP}/{Level * 200}",
            $"Gold {Gold}",
            $"Hit +{Attack + WeaponAttackBonus}  AC {ArmorClass}",
            $"Weapon {WeaponName}",
            $"Skill {SkillName}",
            $"Potions {PotionCount}");

        public string Name => Model.Name;
        public string PlayerClassDisplay => Model.PlayerClass.ToString();
        public int HP => Model.HP;
        public int MaxHP => Model.MaxHP;
        public int Level => Model.Level;
        public int XP => Model.XP;
        public int Gold => Model.Gold;
        public int Attack => Model.Attack;
        public int Defense => Model.Defense;
        public int WeaponAttackBonus => Model.Weapon.AttackBonus;
        public int ArmorClass => Model.ArmorClass;
        public string WeaponName => Model.Weapon.Name;
        public string WeaponDamage => Model.Weapon.Damage.Describe();
        public int PotionCount => Model.Inventory.Items.Count(i => i.ItemType == ItemType.Potion);
        public string SkillName => Model.ClassSkill.Name;
        public string SkillDescription => Model.ClassSkill.Description;

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
                nameof(PlayerClassDisplay),
                nameof(HP),
                nameof(MaxHP),
                nameof(Level),
                nameof(XP),
                nameof(Gold),
                nameof(Attack),
                nameof(Defense),
                nameof(WeaponAttackBonus),
                nameof(ArmorClass),
                nameof(WeaponName),
                nameof(WeaponDamage),
                nameof(PotionCount),
                nameof(SkillName),
                nameof(SkillDescription));
        }
    }
}
