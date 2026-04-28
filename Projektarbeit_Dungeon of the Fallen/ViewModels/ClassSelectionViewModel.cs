using System.Diagnostics;
using System.Windows.Input;
using DungeonOfTheFallen.Core.Models;
using Projektarbeit_Dungeon_of_the_Fallen.Services;

namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    public class ClassSelectionViewModel : ViewModelBase
    {
        private ClassCardViewModel? _selectedCard;
        private ClassCardViewModel? _hoveredCard;

        public IReadOnlyList<ClassCardViewModel> Classes { get; }

        public event Action? BackRequested;
        public event Action<PlayerClass>? StartGameRequested;

        public ICommand BackCommand  { get; }
        public ICommand StartCommand { get; }

        // ── Info-Panel-Bindungen (zeigt hover, sonst ausgewählte Klasse) ──────

        private PlayerClassDefinition? ActiveInfo => _hoveredCard?.Definition ?? _selectedCard?.Definition;

        public string InfoName          => ActiveInfo?.DisplayName      ?? "— Wähle eine Klasse —";
        public string InfoShortDesc     => ActiveInfo?.ShortDescription  ?? "";
        public string InfoDescription   => ActiveInfo?.HoverDescription  ?? "Hover über eine Klasse für Details.";
        public string InfoSpecialsText  => ActiveInfo != null
            ? string.Join("\n", ActiveInfo.Specials.Select(s => "• " + s))
            : "";

        // Rohwerte
        public int InfoHealth  => ActiveInfo?.BaseStats.Health  ?? 0;
        public int InfoDamage  => ActiveInfo?.BaseStats.Damage  ?? 0;
        public int InfoDefense => ActiveInfo?.BaseStats.Defense ?? 0;
        public int InfoSpeed   => ActiveInfo?.BaseStats.Speed   ?? 0;
        public int InfoMagic   => ActiveInfo?.BaseStats.Magic   ?? 0;

        // Prozentwerte für die ProgressBars (Maximum=100)
        public int InfoHealthPct  => InfoHealth;                               // max ≈ 100
        public int InfoDamagePct  => (int)(InfoDamage  / 20.0 * 100);
        public int InfoDefensePct => (int)(InfoDefense / 20.0 * 100);
        public int InfoSpeedPct   => (int)(InfoSpeed   / 10.0 * 100);
        public int InfoMagicPct   => (int)(InfoMagic   / 10.0 * 100);

        public string InfoAccentColor => _hoveredCard?.AccentColor ?? _selectedCard?.AccentColor ?? "#FFD700";

        public bool CanStart         => _selectedCard != null;
        public bool HasActiveInfo    => ActiveInfo != null;

        // ─────────────────────────────────────────────────────────────────────

        public ClassSelectionViewModel()
        {
            var entries = PlayerClassLoader.Load();
            Classes = entries
                .Select(e => new ClassCardViewModel(e.Def, e.Enum, this))
                .ToList();

            BackCommand  = new RelayCommand(_ =>
            {
                Debug.WriteLine("[Menu] Back clicked – returning to main menu");
                BackRequested?.Invoke();
            });
            StartCommand = new RelayCommand(_ =>
            {
                if (_selectedCard == null) return;
                Debug.WriteLine($"[Menu] Starting new game with class: {_selectedCard.Definition.Id}");
                StartGameRequested?.Invoke(_selectedCard.PlayerClass);
            }, _ => CanStart);
        }

        public void SelectCard(ClassCardViewModel card)
        {
            if (_selectedCard != null) _selectedCard.IsSelected = false;
            _selectedCard = card;
            _selectedCard.IsSelected = true;
            Debug.WriteLine($"[ClassSelection] Selected class: {card.Definition.Id}");
            NotifyInfoChanged();
            OnPropertyChanged(nameof(CanStart));
        }

        public void SetHoveredCard(ClassCardViewModel? card)
        {
            _hoveredCard = card;
            if (card != null)
                Debug.WriteLine($"[ClassSelection] Hover class: {card.Definition.Id}");
            NotifyInfoChanged();
        }

        private void NotifyInfoChanged() =>
            OnPropertyChanged(
                nameof(InfoName), nameof(InfoShortDesc), nameof(InfoDescription),
                nameof(InfoSpecialsText),
                nameof(InfoHealth), nameof(InfoDamage), nameof(InfoDefense),
                nameof(InfoSpeed),  nameof(InfoMagic),
                nameof(InfoHealthPct), nameof(InfoDamagePct), nameof(InfoDefensePct),
                nameof(InfoSpeedPct),  nameof(InfoMagicPct),
                nameof(InfoAccentColor), nameof(HasActiveInfo));
    }
}
