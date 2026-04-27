using System.IO;
using System.Windows;
using DungeonOfTheFallen.Core.Models;
using DungeonOfTheFallen.Core.Services;

namespace Projektarbeit_Dungeon_of_the_Fallen
{
    public partial class MainMenuWindow : Window
    {
        private const string SaveFileName = "dungeon_save.xml";

        public MainMenuWindow()
        {
            InitializeComponent();
            LoadGameButton.IsEnabled = File.Exists(SaveFileName);
            ClassSelectionComboBox.ItemsSource = PlayerClassService.GetAllProfiles();
            ClassSelectionComboBox.DisplayMemberPath = nameof(PlayerClassProfile.DisplayName);
            ClassSelectionComboBox.SelectedValuePath = nameof(PlayerClassProfile.PlayerClass);
            ClassSelectionComboBox.SelectedIndex = 0;
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            var selectedClass = ClassSelectionComboBox.SelectedValue is PlayerClass playerClass
                ? playerClass
                : PlayerClass.Warrior;

            var game = new MainWindow(selectedClass);
            Application.Current.MainWindow = game;
            game.Show();
            Close();
        }

        private void LoadGame_Click(object sender, RoutedEventArgs e)
        {
            var game = new MainWindow();
            Application.Current.MainWindow = game;
            game.Show();

            if (game.DataContext is ViewModels.MainViewModel vm)
                vm.LoadGameCommand.Execute(null);

            Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
