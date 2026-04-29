using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Projektarbeit_Dungeon_of_the_Fallen
{
    public partial class MainMenuWindow : Window
    {
        private const string SaveFileName = "dungeon_save.xml";

        public MainMenuWindow()
        {
            InitializeComponent();
            LoadGameButton.IsEnabled = File.Exists(SaveFileName);
            Debug.WriteLine("[Menu] Main menu initialized");
        }

        // ── Fade-in + optionale Nebel-Animation ──────────────────────────

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[Animation] Menu fade-in started");
            var anim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));
            anim.Completed += (_, _) =>
            {
                Debug.WriteLine("[Animation] Menu fade-in finished");
                StartFogAnimation();
            };
            BeginAnimation(OpacityProperty, anim);
            Debug.WriteLine("[Theme] Menu theme loaded");
        }

        private void StartFogAnimation()
        {
            // Fog Layer 1 – langsames Einblenden, läuft nur wenn Bild vorhanden
            var fog1 = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever,
                Duration       = new Duration(TimeSpan.FromSeconds(12))
            };
            fog1.KeyFrames.Add(new EasingDoubleKeyFrame(0,    KeyTime.FromTimeSpan(TimeSpan.Zero)));
            fog1.KeyFrames.Add(new EasingDoubleKeyFrame(0.18, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4))));
            fog1.KeyFrames.Add(new EasingDoubleKeyFrame(0.08, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(8))));
            fog1.KeyFrames.Add(new EasingDoubleKeyFrame(0,    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(12))));
            FogLayer1.BeginAnimation(OpacityProperty, fog1);

            // Fog Layer 2 – zeitversetzt
            var fog2 = new DoubleAnimationUsingKeyFrames
            {
                BeginTime      = TimeSpan.FromSeconds(6),
                RepeatBehavior = RepeatBehavior.Forever,
                Duration       = new Duration(TimeSpan.FromSeconds(12))
            };
            fog2.KeyFrames.Add(new EasingDoubleKeyFrame(0,    KeyTime.FromTimeSpan(TimeSpan.Zero)));
            fog2.KeyFrames.Add(new EasingDoubleKeyFrame(0.14, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4))));
            fog2.KeyFrames.Add(new EasingDoubleKeyFrame(0.06, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(8))));
            fog2.KeyFrames.Add(new EasingDoubleKeyFrame(0,    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(12))));
            FogLayer2.BeginAnimation(OpacityProperty, fog2);
        }

        // ── Button-Handler ────────────────────────────────────────────────

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[Menu] New Game clicked");
            var classSelection = new ClassSelectionWindow();
            Application.Current.MainWindow = classSelection;
            classSelection.Show();
            Close();
        }

        private void LoadGame_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[Menu] Load clicked");
            var game = new MainWindow();
            Application.Current.MainWindow = game;
            game.Show();
            if (game.DataContext is ViewModels.MainViewModel vm)
                vm.LoadGameCommand.Execute(null);
            Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[Menu] Exit clicked");
            var result = MessageBox.Show(
                "Das Verlies wartet noch auf dich.\nWirklich beenden?",
                "Verlies verlassen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("[Menu] DragMove skipped because the primary mouse button was not pressed.");
            }
        }
    }
}
