using System.IO;
using System.Windows;

namespace Projektarbeit_Dungeon_of_the_Fallen
{
    public partial class MainMenuWindow : Window
    {
        private const string SaveFileName = "dungeon_save.xml";

        public MainMenuWindow()
        {
            InitializeComponent();
            LoadGameButton.IsEnabled = File.Exists(SaveFileName);
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            var game = new MainWindow();
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
