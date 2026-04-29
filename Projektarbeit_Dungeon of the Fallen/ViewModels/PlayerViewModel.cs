using DungeonOfTheFallen.Core.Models;
using DungeonOfTheFallen.Core.Services;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        public Player Model { get; }

        public string StatusText => string.Join(
            " | ",
            Name,
            PlayerClassDisplay,
            $"Stufe {Level}  LP {HP}/{MaxHP}",
            $"EP {XP}/{Level * 200}",
            $"Gold {Gold}",
            $"Angriff +{Attack + WeaponAttackBonus}  RK {ArmorClass}",
            $"Waffe {WeaponName}",
            $"Fähigkeit {SkillName}",
            $"Tränke {PotionCount}");

        public string Name => Model.Name;
        public string PlayerClassDisplay => PlayerClassService.GetDisplayName(Model.PlayerClass);
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

        // Rüstung
        public int TotalArmorValue => Model.TotalArmorValue;
        public string ArmorSummary => Model.EquippedArmor.Count == 0
            ? "Keine Rüstung"
            : string.Join(", ", Model.EquippedArmor.Values.Select(a => a.Summary));

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
                nameof(SkillDescription),
                nameof(TotalArmorValue),
                nameof(ArmorSummary));
        }
    }
}
