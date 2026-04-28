using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using DungeonOfTheFallen.Core.Models;
using Projektarbeit_Dungeon_of_the_Fallen.ViewModels;

namespace Projektarbeit_Dungeon_of_the_Fallen
{
    public partial class ClassSelectionWindow : Window
    {
        private readonly ClassSelectionViewModel _vm;

        public ClassSelectionWindow()
        {
            InitializeComponent();
            _vm = new ClassSelectionViewModel();
            DataContext = _vm;

            _vm.BackRequested      += OnBackRequested;
            _vm.StartGameRequested += OnStartGameRequested;

            Debug.WriteLine("[Menu] Opening class selection");
        }

        // ── Navigation ────────────────────────────────────────────────────

        private void OnBackRequested()
        {
            var menu = new MainMenuWindow();
            Application.Current.MainWindow = menu;
            menu.Show();
            Close();
        }

        private void OnStartGameRequested(PlayerClass selectedClass)
        {
            var game = new MainWindow(selectedClass);
            Application.Current.MainWindow = game;
            game.Show();
            Close();
        }

        // ── Hover-Tracking ────────────────────────────────────────────────

        private void ClassCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is System.Windows.Controls.ContentPresenter cp
                && cp.Content is ClassCardViewModel card)
                _vm.SetHoveredCard(card);
        }

        private void ClassCard_MouseLeave(object sender, MouseEventArgs e) =>
            _vm.SetHoveredCard(null);

        // ── Fade-in beim Öffnen ───────────────────────────────────────────

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[Animation] Menu fade-in started");
            var anim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(400)));
            anim.Completed += (_, _) => Debug.WriteLine("[Animation] Menu fade-in finished");
            BeginAnimation(OpacityProperty, anim);
        }

        // ── Fenster verschiebbar ──────────────────────────────────────────

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
            DragMove();
    }
}
