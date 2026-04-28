using System.Windows;
using DungeonOfTheFallen.Core.Models;
using Projektarbeit_Dungeon_of_the_Fallen.ViewModels;

namespace Projektarbeit_Dungeon_of_the_Fallen
{
    public partial class MainWindow : Window
    {
        public MainWindow(PlayerClass selectedClass = PlayerClass.Warrior)
        {
            InitializeComponent();
            var vm = new MainViewModel(selectedClass);
            DataContext = vm;
            vm.CombatRequested           += OnCombatRequested;
            vm.ReturnToMainMenuRequested += OnReturnToMainMenu;
            vm.AbandonRunRequested       += OnAbandonRun;
        }

        private void OnCombatRequested(Enemy enemy)
        {
            if (DataContext is not MainViewModel vm) return;

            var combatVm = new CombatViewModel(vm.GameState, enemy);
            var combatWindow = new CombatWindow(combatVm) { Owner = this };
            combatWindow.ShowDialog();
            // UpdateAllTiles/UpdateCombatLog werden nach ShowDialog vom ViewModel selbst aufgerufen
        }

        private void OnReturnToMainMenu()
        {
            var menu = new MainMenuWindow();
            Application.Current.MainWindow = menu;
            menu.Show();
            Close();
        }

        private void OnAbandonRun()
        {
            var result = MessageBox.Show(
                "Run wirklich abbrechen?\nUngespeicherter Fortschritt geht verloren.",
                "Run abbrechen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var menu = new MainMenuWindow();
                Application.Current.MainWindow = menu;
                menu.Show();
                Close();
            }
        }
    }
}
