using System.Windows;
using Projektarbeit_Dungeon_of_the_Fallen.ViewModels;

namespace Projektarbeit_Dungeon_of_the_Fallen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}