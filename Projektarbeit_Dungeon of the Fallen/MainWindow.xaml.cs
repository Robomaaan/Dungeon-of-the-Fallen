using System.Windows;
using DungeonOfTheFallen.Core.Models;
using Projektarbeit_Dungeon_of_the_Fallen.ViewModels;

namespace Projektarbeit_Dungeon_of_the_Fallen
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            DataContext = vm;
            vm.CombatRequested += OnCombatRequested;
        }

        private void OnCombatRequested(Enemy enemy)
        {
            if (DataContext is not MainViewModel vm) return;

            var combatVm = new CombatViewModel(vm.GameState, enemy);
            var combatWindow = new CombatWindow(combatVm)
            {
                Owner = this
            };
            combatWindow.ShowDialog();
        }
    }
}
