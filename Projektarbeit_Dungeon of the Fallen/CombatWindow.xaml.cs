using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DungeonOfTheFallen.Core.Models;
using Projektarbeit_Dungeon_of_the_Fallen.ViewModels;

namespace Projektarbeit_Dungeon_of_the_Fallen
{
    public partial class CombatWindow : Window
    {
        private CombatViewModel _vm;
        private int _pendingDiceValue;
        private bool _pendingIsPlayer;
        private bool _animationRunning;

        public CombatWindow(CombatViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = vm;

            vm.DiceRollStarted += OnDiceRollStarted;
            vm.PropertyChanged += Vm_PropertyChanged;

            BuildPlayerSprite();
            BuildEnemySprite(vm.EnemyType);
        }

        // ── Sprite Builders ──────────────────────────────────────────────

        private void BuildPlayerSprite()
        {
            var c = PlayerSpriteCanvas;
            c.Children.Clear();

            c.Children.Add(Rect(32, 0, 10, 18, "#CC2222", rx: 4));
            c.Children.Add(Rect(18, 14, 46, 20, "#8899AA", rx: 8));
            c.Children.Add(MakeEllipse(24, 20, 44, 34, "#DEB887"));
            c.Children.Add(Rect(20, 30, 52, 7, "#222233", opacity: 0.85));
            c.Children.Add(MakeEllipse(2, 50, 26, 20, "#4455AA"));
            c.Children.Add(MakeEllipse(64, 50, 26, 20, "#4455AA"));
            c.Children.Add(Rect(18, 54, 56, 48, "#3344AA", rx: 4));
            c.Children.Add(Rect(42, 60, 8, 30, "#FFD700", opacity: 0.55));
            c.Children.Add(Rect(30, 72, 32, 8, "#FFD700", opacity: 0.55));
            c.Children.Add(MakeEllipseWithStroke(0, 54, 22, 34, "#881111", "#FF4444", 2));
            c.Children.Add(Rect(88, 12, 7, 70, "#CCDDEE", rx: 2));
            c.Children.Add(Rect(82, 58, 20, 6, "#887766", rx: 2));
            c.Children.Add(MakePath("M14,56 C8,90 4,115 14,155 L26,155 L26,104 Z", "#5522AA", 0.7));
            c.Children.Add(MakePath("M78,56 C86,90 90,115 80,155 L68,155 L68,104 Z", "#5522AA", 0.7));
            c.Children.Add(Rect(18, 104, 26, 42, "#2233AA", rx: 3));
            c.Children.Add(Rect(48, 104, 26, 42, "#2233AA", rx: 3));
            c.Children.Add(Rect(14, 140, 32, 12, "#111122", rx: 4));
            c.Children.Add(Rect(46, 140, 32, 12, "#111122", rx: 4));
        }

        private void BuildEnemySprite(EnemyType type)
        {
            EnemySpriteCanvas.Children.Clear();
            switch (type)
            {
                case EnemyType.Goblin: BuildGoblin(); break;
                case EnemyType.Spider: BuildSpider(); break;
                case EnemyType.Skeleton: BuildSkeleton(); break;
                case EnemyType.Orc:    BuildOrc();    break;
                case EnemyType.Zombie: BuildZombie(); break;
                case EnemyType.Troll: BuildTroll(); break;
                case EnemyType.Dragon:
                case EnemyType.DemonLord:
                case EnemyType.Lich:
                case EnemyType.Boss:   BuildDragon(); break;
                default:               BuildGoblin(); break;
            }
        }

        private void BuildSpider()
        {
            var c = EnemySpriteCanvas;
            c.Children.Add(MakeEllipse(20, 40, 56, 42, "#1A1A1A"));
            c.Children.Add(MakeEllipse(32, 20, 34, 28, "#222244"));
            c.Children.Add(MakeEllipse(38, 28, 6, 6, "#FF3333"));
            c.Children.Add(MakeEllipse(54, 28, 6, 6, "#FF3333"));
            c.Children.Add(MakePath("M20,50 L0,30", "#444466", 1.0, true, 4));
            c.Children.Add(MakePath("M24,58 L0,58", "#444466", 1.0, true, 4));
            c.Children.Add(MakePath("M26,68 L4,86", "#444466", 1.0, true, 4));
            c.Children.Add(MakePath("M76,50 L96,30", "#444466", 1.0, true, 4));
            c.Children.Add(MakePath("M72,58 L96,58", "#444466", 1.0, true, 4));
            c.Children.Add(MakePath("M70,68 L92,86", "#444466", 1.0, true, 4));
        }

        private void BuildSkeleton()
        {
            var c = EnemySpriteCanvas;
            c.Children.Add(MakeEllipse(26, 6, 44, 36, "#D9D9D9"));
            c.Children.Add(MakeEllipse(38, 18, 6, 6, "#111111"));
            c.Children.Add(MakeEllipse(52, 18, 6, 6, "#111111"));
            c.Children.Add(Rect(42, 40, 10, 52, "#CCCCCC", rx: 3));
            c.Children.Add(Rect(26, 56, 42, 10, "#CCCCCC", rx: 3));
            c.Children.Add(Rect(16, 52, 10, 38, "#CCCCCC", rx: 3));
            c.Children.Add(Rect(68, 52, 10, 38, "#CCCCCC", rx: 3));
            c.Children.Add(Rect(30, 92, 10, 40, "#CCCCCC", rx: 3));
            c.Children.Add(Rect(54, 92, 10, 40, "#CCCCCC", rx: 3));
        }

        private void BuildGoblin()
        {
            var c = EnemySpriteCanvas;
            c.Children.Add(MakeEllipseWithStroke(-4, 8, 18, 14, "#2a7a1a", null, 0));
            c.Children.Add(MakeEllipseWithStroke(80, 8, 18, 14, "#2a7a1a", null, 0));
            c.Children.Add(MakeEllipse(12, 4, 56, 46, "#3a9a2a"));
            c.Children.Add(MakeEllipse(22, 14, 12, 10, "#FFEE00"));
            c.Children.Add(MakeEllipse(50, 14, 12, 10, "#FFEE00"));
            c.Children.Add(MakeEllipse(25, 16, 6, 7, "#111111"));
            c.Children.Add(MakeEllipse(53, 16, 6, 7, "#111111"));
            c.Children.Add(MakeEllipse(34, 28, 12, 9, "#2a7a1a"));
            c.Children.Add(Rect(24, 40, 36, 8, "#EEEECC", rx: 2));
            c.Children.Add(Rect(31, 39, 6, 12, "#EEEECC", rx: 2));
            c.Children.Add(Rect(47, 39, 6, 12, "#EEEECC", rx: 2));
            c.Children.Add(Rect(18, 50, 48, 44, "#2a8a1a", rx: 5));
            c.Children.Add(Rect(4, 54, 14, 32, "#3a9a2a", rx: 4));
            c.Children.Add(Rect(66, 54, 14, 32, "#3a9a2a", rx: 4));
            c.Children.Add(Rect(70, 30, 10, 28, "#7a4010", rx: 4));
            c.Children.Add(MakeEllipse(66, 16, 18, 18, "#8a5010"));
            c.Children.Add(Rect(20, 94, 20, 28, "#1a6a0a", rx: 3));
            c.Children.Add(Rect(44, 94, 20, 28, "#1a6a0a", rx: 3));
        }

        private void BuildOrc()
        {
            var c = EnemySpriteCanvas;
            c.Width = 130;
            c.Children.Add(Rect(20, 6, 68, 54, "#6a5a2a", rx: 5));
            c.Children.Add(Rect(28, 48, 12, 16, "#DDDDC0", rx: 2));
            c.Children.Add(Rect(62, 48, 12, 16, "#DDDDC0", rx: 2));
            c.Children.Add(MakeEllipse(28, 18, 18, 13, "#FF2200"));
            c.Children.Add(MakeEllipse(62, 18, 18, 13, "#FF2200"));
            c.Children.Add(MakeEllipse(32, 20, 8, 8, "#111111"));
            c.Children.Add(MakeEllipse(66, 20, 8, 8, "#111111"));
            c.Children.Add(MakePath("M55,10 L65,30", "#551100", 1.0, isStroke: true, sw: 2.5));
            c.Children.Add(Rect(10, 62, 90, 62, "#5a4a1a", rx: 5));
            c.Children.Add(MakeEllipse(0, 54, 30, 24, "#6a5a2a"));
            c.Children.Add(MakeEllipse(88, 54, 30, 24, "#6a5a2a"));
            c.Children.Add(Rect(-4, 62, 20, 50, "#6a5a2a", rx: 5));
            c.Children.Add(Rect(92, 62, 20, 50, "#6a5a2a", rx: 5));
            c.Children.Add(Rect(104, 30, 10, 60, "#6a3a0a", rx: 3));
            c.Children.Add(MakePath("M100,20 L128,16 L128,52 L100,44 Z", "#888888", 1.0));
            c.Children.Add(Rect(14, 124, 36, 36, "#4a3a0a", rx: 4));
            c.Children.Add(Rect(58, 124, 36, 36, "#4a3a0a", rx: 4));
        }

        private void BuildZombie()
        {
            var c = EnemySpriteCanvas;
            c.Children.Add(MakeEllipse(22, 6, 52, 42, "#6F8A5A"));
            c.Children.Add(MakeEllipse(34, 20, 8, 8, "#FFDD88"));
            c.Children.Add(MakeEllipse(54, 20, 8, 8, "#FFDD88"));
            c.Children.Add(Rect(18, 48, 60, 60, "#4A5F3A", rx: 6));
            c.Children.Add(Rect(6, 54, 16, 44, "#6F8A5A", rx: 4));
            c.Children.Add(Rect(74, 54, 16, 44, "#6F8A5A", rx: 4));
            c.Children.Add(Rect(24, 108, 18, 42, "#3B4A30", rx: 4));
            c.Children.Add(Rect(54, 108, 18, 42, "#3B4A30", rx: 4));
        }

        private void BuildTroll()
        {
            var c = EnemySpriteCanvas;
            c.Children.Add(MakeEllipse(18, 0, 60, 48, "#5C7A46"));
            c.Children.Add(MakeEllipse(28, 16, 10, 10, "#FF6600"));
            c.Children.Add(MakeEllipse(56, 16, 10, 10, "#FF6600"));
            c.Children.Add(Rect(10, 46, 76, 76, "#476235", rx: 8));
            c.Children.Add(Rect(0, 54, 18, 52, "#5C7A46", rx: 4));
            c.Children.Add(Rect(78, 54, 18, 52, "#5C7A46", rx: 4));
            c.Children.Add(Rect(24, 120, 20, 42, "#324824", rx: 4));
            c.Children.Add(Rect(54, 120, 20, 42, "#324824", rx: 4));
            c.Children.Add(Rect(86, 20, 10, 62, "#7A4A1A", rx: 3));
            c.Children.Add(MakeEllipse(80, 10, 24, 24, "#8A5A2A"));
        }

        private void BuildDragon()
        {
            var c = EnemySpriteCanvas;
            c.Width = 160;
            c.Children.Add(MakePath("M0,70 L44,4 L64,44 L44,68 Z", "#880022", 0.9));
            c.Children.Add(MakePath("M160,70 L116,4 L96,44 L116,68 Z", "#880022", 0.9));
            c.Children.Add(MakeEllipse(44, 56, 80, 90, "#AA0033"));
            c.Children.Add(Rect(62, 28, 40, 36, "#AA0033", rx: 10));
            c.Children.Add(MakeEllipse(46, 4, 78, 52, "#CC0044"));
            c.Children.Add(MakePath("M52,4 L44,0 L60,20 Z", "#550011", 1.0));
            c.Children.Add(MakePath("M108,4 L116,0 L100,20 Z", "#550011", 1.0));
            c.Children.Add(MakeEllipse(58, 14, 24, 16, "#FF8800"));
            c.Children.Add(MakeEllipse(92, 14, 24, 16, "#FF8800"));
            c.Children.Add(MakeEllipse(63, 17, 12, 12, "#FF0000"));
            c.Children.Add(MakeEllipse(97, 17, 12, 12, "#FF0000"));
            c.Children.Add(MakeEllipse(68, 38, 8, 6, "#550011"));
            c.Children.Add(MakeEllipse(98, 38, 8, 6, "#550011"));
            c.Children.Add(Rect(60, 46, 54, 8, "#111111", opacity: 0.6, rx: 2));
            c.Children.Add(Rect(68, 47, 7, 12, "#EEEEEE", rx: 2));
            c.Children.Add(Rect(80, 47, 7, 12, "#EEEEEE", rx: 2));
            c.Children.Add(Rect(92, 47, 7, 12, "#EEEEEE", rx: 2));
            c.Children.Add(MakePath("M124,100 C145,110 155,130 148,155", "#880022", 1.0, isStroke: true, sw: 16));
            c.Children.Add(Rect(50, 140, 30, 20, "#880022", rx: 4));
            c.Children.Add(Rect(90, 140, 30, 20, "#880022", rx: 4));
            c.Children.Add(MakePath("M44,158 L50,140 L58,158 Z", "#AA0033", 1.0));
            c.Children.Add(MakePath("M92,158 L100,140 L108,158 Z", "#AA0033", 1.0));
        }

        // ── Shape helpers ─────────────────────────────────────────────────

        private static UIElement Rect(double l, double t, double w, double h,
            string fill, double rx = 0, double opacity = 1.0)
        {
            var r = new Rectangle
            {
                Width = w, Height = h,
                Fill = Brush(fill),
                RadiusX = rx, RadiusY = rx,
                Opacity = opacity
            };
            Set(r, l, t);
            return r;
        }

        private static UIElement MakeEllipse(double l, double t, double w, double h, string fill)
        {
            var e = new Ellipse { Width = w, Height = h, Fill = Brush(fill) };
            Set(e, l, t);
            return e;
        }

        private static UIElement MakeEllipseWithStroke(double l, double t, double w, double h,
            string fill, string? stroke, double sw)
        {
            var e = new Ellipse { Width = w, Height = h, Fill = Brush(fill) };
            if (stroke != null) { e.Stroke = Brush(stroke); e.StrokeThickness = sw; }
            Set(e, l, t);
            return e;
        }

        private static UIElement MakePath(string data, string fill, double opacity,
            bool isStroke = false, double sw = 1)
        {
            var p = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(data),
                Fill = isStroke ? Brushes.Transparent : Brush(fill),
                Opacity = opacity
            };
            if (isStroke) { p.Stroke = Brush(fill); p.StrokeThickness = sw; }
            return p;
        }

        private static SolidColorBrush Brush(string hex) =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

        private static void Set(UIElement e, double l, double t)
        {
            System.Windows.Controls.Canvas.SetLeft(e, l);
            System.Windows.Controls.Canvas.SetTop(e, t);
        }

        // ── Dice Animation ────────────────────────────────────────────────
        // Uses BeginAnimation directly on named transforms — no Storyboard.Begin()
        // needed, avoiding the "no FrameworkElement" exception.

        private void OnDiceRollStarted(int value, bool isPlayer)
        {
            if (_animationRunning) return;
            _animationRunning = true;
            _pendingDiceValue = value;
            _pendingIsPlayer = isPlayer;

            DiceLabel.Text = isPlayer ? "Du würfelst..." : $"{_vm.EnemyName} würfelt...";
            DiceResultLabel.Text = "";
            HideDots();

            // Reset transforms to base values
            DiceTranslate.BeginAnimation(TranslateTransform.YProperty, null);
            DiceTranslate.Y = 0;
            DiceRotate.BeginAnimation(RotateTransform.AngleProperty, null);
            DiceRotate.Angle = 0;
            DiceScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            DiceScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            DiceScale.ScaleX = DiceScale.ScaleY = 1;

            BeginThrowAnimation();
        }

        private void BeginThrowAnimation()
        {
            // Y translation: fly up then bounce down
            var yAnim = new DoubleAnimationUsingKeyFrames();
            yAnim.KeyFrames.Add(new EasingDoubleKeyFrame(-140,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(360)))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
            yAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(780)))
                { EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2.5, EasingMode = EasingMode.EaseOut } });
            DiceTranslate.BeginAnimation(TranslateTransform.YProperty, yAnim);

            // Rotation: 2 full spins, resets to 0 via FillBehavior.Stop
            var rotAnim = new DoubleAnimation(0, 720, TimeSpan.FromMilliseconds(780))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
                FillBehavior = FillBehavior.Stop
            };
            rotAnim.Completed += (_, _) => Dispatcher.Invoke(OnThrowComplete);
            DiceRotate.BeginAnimation(RotateTransform.AngleProperty, rotAnim);
        }

        private void OnThrowComplete()
        {
            // Rotation animation stopped — angle is back to local value 0
            ShowDots(_pendingDiceValue);
            DiceResultLabel.Text = _pendingIsPlayer
                ? $"Du würfelst eine {_pendingDiceValue}!"
                : $"{_vm.EnemyName} würfelt eine {_pendingDiceValue}!";

            BeginZoomAnimation();
        }

        private void BeginZoomAnimation()
        {
            // Scale: flash up then elastic settle back to 1
            var keyTimes = new[]
            {
                (1.75, TimeSpan.FromMilliseconds(160), (IEasingFunction)new CubicEase { EasingMode = EasingMode.EaseOut }),
                (1.0,  TimeSpan.FromMilliseconds(500), (IEasingFunction)new ElasticEase { Oscillations = 1, EasingMode = EasingMode.EaseOut })
            };

            DoubleAnimationUsingKeyFrames MakeScaleAnim()
            {
                var a = new DoubleAnimationUsingKeyFrames { FillBehavior = FillBehavior.Stop };
                foreach (var (val, t, ease) in keyTimes)
                    a.KeyFrames.Add(new EasingDoubleKeyFrame(val, KeyTime.FromTimeSpan(t)) { EasingFunction = ease });
                return a;
            }

            var animX = MakeScaleAnim();
            var animY = MakeScaleAnim();
            animY.Completed += (_, _) => Dispatcher.Invoke(OnZoomComplete);

            DiceScale.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
            DiceScale.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
        }

        private void OnZoomComplete()
        {
            _animationRunning = false;
            if (_pendingIsPlayer)
                _vm.OnPlayerDiceAnimationComplete();
            else
                _vm.OnEnemyDiceAnimationComplete();
        }

        // ── Dice face dots ────────────────────────────────────────────────

        private void HideDots()
        {
            foreach (var dot in new[] { Dot_TL, Dot_TR, Dot_ML, Dot_C, Dot_MR, Dot_BL, Dot_BR })
                dot.Visibility = Visibility.Collapsed;
        }

        private void ShowDots(int value)
        {
            HideDots();
            switch (value)
            {
                case 1: Show(Dot_C); break;
                case 2: Show(Dot_TR, Dot_BL); break;
                case 3: Show(Dot_TR, Dot_C, Dot_BL); break;
                case 4: Show(Dot_TL, Dot_TR, Dot_BL, Dot_BR); break;
                case 5: Show(Dot_TL, Dot_TR, Dot_C, Dot_BL, Dot_BR); break;
                case 6: Show(Dot_TL, Dot_TR, Dot_ML, Dot_MR, Dot_BL, Dot_BR); break;
            }
        }

        private static void Show(params UIElement[] elements)
        {
            foreach (var e in elements) e.Visibility = Visibility.Visible;
        }

        // ── ViewModel observer ────────────────────────────────────────────

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CombatViewModel.IsGameOver) && _vm.IsGameOver)
            {
                CloseButton.Visibility = Visibility.Visible;
                DiceLabel.Text = _vm.IsVictory ? "🏆 Sieg!" : "💀 Niederlage";
            }
        }

        private void CombatWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_vm.IsGameOver)
            {
                e.Cancel = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
