using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using DungeonOfTheFallen.Core.Models;
using Projektarbeit_Dungeon_of_the_Fallen.ViewModels;

namespace Projektarbeit_Dungeon_of_the_Fallen
{
    public partial class CombatWindow : Window
    {
        private readonly CombatViewModel _vm;
        private bool _animationRunning;

        // ── Attack animation state ────────────────────────────────────────
        private DispatcherTimer? _attackTimer;
        private string[]?        _attackFrames;
        private int              _attackFrameIndex;
        private Canvas?          _attackCanvas;
        private Action?          _attackDoneCallback;

        // ── Dice battle animation ─────────────────────────────────────────
        private int _pendingPlayerRoll;
        private int _pendingEnemyRoll;
        private int _throwCompletions;

        private DispatcherTimer? _playerShuffleTimer;
        private DispatcherTimer? _enemyShuffleTimer;

        private static readonly Random _rng = new();

        // Distanz, die jeder Würfel zur Mitte zurücklegt (muss zur XAML-Position passen)
        // PlayerDie Canvas.Left=60, EnemyDie Canvas.Left=500, Canvas.Width=680
        // Kollisionspunkt = 340 (Mitte). PlayerDie linke Kante → 220, delta=+160
        // EnemyDie linke Kante → 340, delta=-160
        private const double CollisionDisplacement = 160.0;

        // ── Asset paths ───────────────────────────────────────────────────
        private static readonly string[] DiceFramePaths =
            Enumerable.Range(1, 20).Select(i => $"/Assets/Dice/d20_{i:D2}.png").ToArray();

        private static readonly string[] DiceBreakFramePaths =
        {
            "/Assets/Dice/d20_break1.png",
            "/Assets/Dice/d20_break2.png",
            "/Assets/Dice/d20_break3.png",
        };

        private static readonly ConcurrentDictionary<string, BitmapImage?> _spriteCache = new();

        // ── Attack frame sets ─────────────────────────────────────────────
        private static readonly string[] PlayerAttackFrames =
        {
            "/Assets/Characters/Player/player_attack_down_00.png",
            "/Assets/Characters/Player/player_attack_down_01.png",
            "/Assets/Characters/Player/player_attack_down_02.png",
            "/Assets/Characters/Player/player_attack_down_03.png",
        };

        private static string[]? GetEnemyAttackFrames(EnemyType type) => type switch
        {
            EnemyType.Goblin => new[]
            {
                "/Assets/Characters/Goblin/goblin_attack_down_01.png",
                "/Assets/Characters/Goblin/goblin_attack_down_02.png",
                "/Assets/Characters/Goblin/goblin_attack_down_03.png",
            },
            EnemyType.Orc or EnemyType.Troll => new[]
            {
                "/Assets/Characters/Orc/orc_attack_down_00.png",
                "/Assets/Characters/Orc/orc_attack_down_01.png",
                "/Assets/Characters/Orc/orc_attack_down_02.png",
                "/Assets/Characters/Orc/orc_attack_down_03.png",
            },
            EnemyType.Dragon or EnemyType.DemonLord or EnemyType.Lich or EnemyType.Boss => new[]
            {
                "/Assets/Characters/Boss/boss_orc_attack_down_00.png",
                "/Assets/Characters/Boss/boss_orc_attack_down_01.png",
                "/Assets/Characters/Boss/boss_orc_attack_down_02.png",
                "/Assets/Characters/Boss/boss_orc_attack_down_03.png",
            },
            _ => null
        };

        // ═════════════════════════════════════════════════════════════════
        //  Constructor
        // ═════════════════════════════════════════════════════════════════

        public CombatWindow(CombatViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = vm;

            vm.DiceBattleStarted += OnDiceBattleStarted;
            vm.DiceRollStarted   += OnDiceRollStarted;   // Trank-Pfad
            vm.PropertyChanged   += Vm_PropertyChanged;

            BuildPlayerSprite();
            BuildEnemySprite(vm.EnemyType);

            Debug.WriteLine("[UIScale] Combat scene scaling initialized");
            Debug.WriteLine($"[UIScale] Player sprite size: {PlayerSpriteCanvas.Width}x{PlayerSpriteCanvas.Height}");
            Debug.WriteLine($"[UIScale] Enemy sprite size:  {EnemySpriteCanvas.Width}x{EnemySpriteCanvas.Height}");
            Debug.WriteLine($"[UIScale] Dice size: 120x120 (each)");
            Debug.WriteLine($"[UIScale] Window size: {Width}x{Height}");
            Debug.WriteLine("[UIScale] Layout adjustment finished");
        }

        // ═════════════════════════════════════════════════════════════════
        //  Sprite builders
        // ═════════════════════════════════════════════════════════════════

        private void BuildPlayerSprite()
        {
            var c = PlayerSpriteCanvas;
            c.Children.Clear();

            if (TryAddSpriteImage(c, "/Assets/Characters/Player/player_idle_down_00.png"))
                return;

            // Prozeduraler Fallback (skaliert für neuen Canvas)
            c.RenderTransform = new ScaleTransform(190.0 / 110, 260.0 / 160);
            c.RenderTransformOrigin = new Point(0, 0);
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
            var c = EnemySpriteCanvas;
            c.Children.Clear();
            c.RenderTransform = null;

            var spritePath = type switch
            {
                EnemyType.Goblin   => "/Assets/Characters/Goblin/goblin_idle_down_00.png",
                EnemyType.Orc      => "/Assets/Characters/Orc/orc_idle_down_00.png",
                EnemyType.Ogre     => "/Assets/Characters/Ogre/ogre_idle_down_00.png",
                EnemyType.Dragon or EnemyType.DemonLord or EnemyType.Lich or EnemyType.Boss
                    => "/Assets/Characters/Boss/boss_orc_idle_down_00.png",
                _ => null
            };

            if (spritePath != null && TryAddSpriteImage(c, spritePath))
                return;

            // Prozeduraler Fallback skalieren
            c.RenderTransform = new ScaleTransform(190.0 / 130, 260.0 / 160);
            c.RenderTransformOrigin = new Point(0, 0);

            switch (type)
            {
                case EnemyType.Goblin:   BuildGoblin();   break;
                case EnemyType.Spider:   BuildSpider();   break;
                case EnemyType.Skeleton: BuildSkeleton(); break;
                case EnemyType.Orc:      BuildOrc();      break;
                case EnemyType.Zombie:   BuildZombie();   break;
                case EnemyType.Troll:    BuildTroll();    break;
                case EnemyType.Ogre:     BuildOgre();     break;
                case EnemyType.Dragon:
                case EnemyType.DemonLord:
                case EnemyType.Lich:
                case EnemyType.Boss:     BuildDragon();   break;
                default:                 BuildGoblin();   break;
            }
        }

        // ═════════════════════════════════════════════════════════════════
        //  Attack animation
        // ═════════════════════════════════════════════════════════════════

        private void AnimateAttack(Canvas canvas, string[] frames, Action onDone)
        {
            _attackTimer?.Stop();
            _attackCanvas       = canvas;
            _attackFrames       = frames;
            _attackFrameIndex   = 0;
            _attackDoneCallback = onDone;

            ShowSpriteFrame(canvas, frames[0]);
            _attackFrameIndex = 1;

            _attackTimer       = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(160) };
            _attackTimer.Tick += OnAttackFrameTick;
            _attackTimer.Start();
        }

        private void OnAttackFrameTick(object? sender, EventArgs e)
        {
            if (_attackFrameIndex >= _attackFrames!.Length)
            {
                _attackTimer!.Stop();
                _attackTimer.Tick -= OnAttackFrameTick;
                _attackDoneCallback?.Invoke();
                return;
            }
            ShowSpriteFrame(_attackCanvas!, _attackFrames[_attackFrameIndex++]);
        }

        private static void ShowSpriteFrame(Canvas canvas, string path)
        {
            var bitmap = LoadBitmap(path);
            if (bitmap == null) return;
            var img = canvas.Children.OfType<System.Windows.Controls.Image>().FirstOrDefault();
            if (img != null) img.Source = bitmap;
        }

        private static bool TryAddSpriteImage(Canvas canvas, string path)
        {
            var bitmap = LoadBitmap(path);
            if (bitmap == null) return false;

            var img = new System.Windows.Controls.Image
            {
                Source  = bitmap,
                Width   = canvas.Width,
                Height  = canvas.Height,
                Stretch = Stretch.Uniform
            };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            Canvas.SetLeft(img, 0);
            Canvas.SetTop(img, 0);
            canvas.Children.Add(img);
            return true;
        }

        // ═════════════════════════════════════════════════════════════════
        //  Dice Battle Animation  – SIMULTANE ANIMATION BEIDER WÜRFEL
        // ═════════════════════════════════════════════════════════════════

        private void OnDiceBattleStarted(int playerRoll, int enemyRoll)
        {
            if (_animationRunning) return;
            _animationRunning  = true;
            _pendingPlayerRoll = playerRoll;
            _pendingEnemyRoll  = enemyRoll;
            _throwCompletions  = 0;

            Debug.WriteLine("[DiceAnimation] Both dice launched");

            DiceLabel.Text        = "Würfel fliegen!";
            DiceResultLabel.Text  = "";
            DiceCompareLabel.Text = "";

            // Würfel zurücksetzen und einblenden
            ResetDiceTransforms();
            PlayerDieBorder.Visibility = Visibility.Visible;
            EnemyDieBorder.Visibility  = Visibility.Visible;
            PlayerDiceImage.Source     = null;
            EnemyDiceImage.Source      = null;

            ShowDiceFrame(PlayerDiceImage, _rng.Next(0, 20));
            ShowDiceFrame(EnemyDiceImage,  _rng.Next(0, 20));

            // Shuffle starten
            StartShuffleBoth();

            // Charakter-Angriffs-Animationen parallel starten
            AnimateAttack(PlayerSpriteCanvas, PlayerAttackFrames, () => BuildPlayerSprite());
            var frames = GetEnemyAttackFrames(_vm.EnemyType);
            if (frames != null)
                AnimateAttack(EnemySpriteCanvas, frames, () => BuildEnemySprite(_vm.EnemyType));

            // Würfeln: Spieler dreht rechts, Gegner dreht links
            BeginThrowAnimation(
                PlayerDiceRotate, PlayerDiceTranslate, PlayerDiceScale,
                +720.0, OnThrowPartComplete);
            BeginThrowAnimation(
                EnemyDiceRotate,  EnemyDiceTranslate,  EnemyDiceScale,
                -720.0, OnThrowPartComplete);
        }

        // Trank-Pfad: nur Gegner-Würfel (Single-Die-Fallback)
        private void OnDiceRollStarted(int value, bool isPlayer)
        {
            if (!isPlayer) OnSingleEnemyDiceStarted(value);
        }

        private void OnSingleEnemyDiceStarted(int enemyRoll)
        {
            if (_animationRunning) return;
            _animationRunning = true;
            _pendingEnemyRoll = enemyRoll;

            DiceLabel.Text        = $"{_vm.EnemyName} würfelt...";
            DiceResultLabel.Text  = "";
            DiceCompareLabel.Text = "";

            ResetDiceTransforms();
            PlayerDieBorder.Visibility = Visibility.Hidden;
            EnemyDieBorder.Visibility  = Visibility.Visible;
            EnemyDiceImage.Source      = null;
            ShowDiceFrame(EnemyDiceImage, _rng.Next(0, 20));

            _enemyShuffleTimer?.Stop();
            _enemyShuffleTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(75) };
            _enemyShuffleTimer.Tick += (_, _) => ShowDiceFrame(EnemyDiceImage, _rng.Next(0, 20));
            _enemyShuffleTimer.Start();

            var yAnim = new DoubleAnimationUsingKeyFrames();
            yAnim.KeyFrames.Add(new EasingDoubleKeyFrame(-90,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300)))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
            yAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(660)))
                { EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2.5, EasingMode = EasingMode.EaseOut } });
            EnemyDiceTranslate.BeginAnimation(TranslateTransform.YProperty, yAnim);

            var rotAnim = new DoubleAnimation(0, -540, TimeSpan.FromMilliseconds(660))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
                FillBehavior   = FillBehavior.Stop
            };
            rotAnim.Completed += (_, _) => Dispatcher.Invoke(() =>
            {
                _enemyShuffleTimer?.Stop();
                ShowDiceFrame(EnemyDiceImage, enemyRoll - 1);
                DiceResultLabel.Text = $"{_vm.EnemyName} würfelt eine {enemyRoll}!";

                var zX = MakeZoomAnim(false, null);
                var zY = MakeZoomAnim(true,  () => Dispatcher.Invoke(() =>
                {
                    _animationRunning = false;
                    _vm.OnEnemyDiceAnimationComplete();
                }));
                EnemyDiceScale.BeginAnimation(ScaleTransform.ScaleXProperty, zX);
                EnemyDiceScale.BeginAnimation(ScaleTransform.ScaleYProperty, zY);
            });
            EnemyDiceRotate.BeginAnimation(RotateTransform.AngleProperty, rotAnim);
        }

        // ── Transforms zurücksetzen ───────────────────────────────────────

        private void ResetDiceTransforms()
        {
            void Reset(ScaleTransform s, RotateTransform r, TranslateTransform t)
            {
                t.BeginAnimation(TranslateTransform.XProperty, null); t.X = 0;
                t.BeginAnimation(TranslateTransform.YProperty, null); t.Y = 0;
                r.BeginAnimation(RotateTransform.AngleProperty, null); r.Angle = 0;
                s.BeginAnimation(ScaleTransform.ScaleXProperty, null); s.ScaleX = 1;
                s.BeginAnimation(ScaleTransform.ScaleYProperty, null); s.ScaleY = 1;
            }
            Reset(PlayerDiceScale, PlayerDiceRotate, PlayerDiceTranslate);
            Reset(EnemyDiceScale,  EnemyDiceRotate,  EnemyDiceTranslate);
        }

        // ── Shuffle ───────────────────────────────────────────────────────

        private void StartShuffleBoth()
        {
            _playerShuffleTimer?.Stop();
            _enemyShuffleTimer?.Stop();

            _playerShuffleTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(75) };
            _playerShuffleTimer.Tick += (_, _) => ShowDiceFrame(PlayerDiceImage, _rng.Next(0, 20));
            _playerShuffleTimer.Start();

            _enemyShuffleTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(75) };
            _enemyShuffleTimer.Tick += (_, _) => ShowDiceFrame(EnemyDiceImage, _rng.Next(0, 20));
            _enemyShuffleTimer.Start();
        }

        // ── Wurf-Animation (parametrisiert für beide Würfel) ──────────────

        private void BeginThrowAnimation(
            RotateTransform rot, TranslateTransform trans, ScaleTransform _scale,
            double rotationDeg, Action onComplete)
        {
            // Hoch und zurückbouncen
            var yAnim = new DoubleAnimationUsingKeyFrames();
            yAnim.KeyFrames.Add(new EasingDoubleKeyFrame(-90,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300)))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
            yAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(660)))
                { EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2.5, EasingMode = EasingMode.EaseOut } });
            trans.BeginAnimation(TranslateTransform.YProperty, yAnim);

            var rotAnim = new DoubleAnimation(0, rotationDeg, TimeSpan.FromMilliseconds(660))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
                FillBehavior   = FillBehavior.Stop
            };
            rotAnim.Completed += (_, _) => Dispatcher.Invoke(onComplete);
            rot.BeginAnimation(RotateTransform.AngleProperty, rotAnim);
        }

        // Wird von beiden Rotations-Completed aufgerufen; läuft erst weiter wenn beide fertig
        private void OnThrowPartComplete()
        {
            _throwCompletions++;
            if (_throwCompletions < 2) return;

            _playerShuffleTimer?.Stop();
            _enemyShuffleTimer?.Stop();

            ShowDiceFrame(PlayerDiceImage, _pendingPlayerRoll - 1);
            ShowDiceFrame(EnemyDiceImage,  _pendingEnemyRoll  - 1);

            DiceResultLabel.Text = $"Spieler: {_pendingPlayerRoll}   vs   Gegner: {_pendingEnemyRoll}";
            Debug.WriteLine("[DiceAnimation] Values shown");

            // Zoom, dann Pause, dann Kollision
            BeginZoomBoth(() =>
            {
                var pause = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(380) };
                pause.Tick += (_, _) => { pause.Stop(); BeginCollisionAnimation(); };
                pause.Start();
            });
        }

        // ── Zoom beider Würfel ────────────────────────────────────────────

        private void BeginZoomBoth(Action onComplete)
        {
            PlayerDiceScale.BeginAnimation(ScaleTransform.ScaleXProperty, MakeZoomAnim(false, null));
            PlayerDiceScale.BeginAnimation(ScaleTransform.ScaleYProperty, MakeZoomAnim(false, null));
            EnemyDiceScale.BeginAnimation(ScaleTransform.ScaleXProperty,  MakeZoomAnim(false, null));
            EnemyDiceScale.BeginAnimation(ScaleTransform.ScaleYProperty,
                MakeZoomAnim(true, () => Dispatcher.Invoke(onComplete)));
        }

        private static DoubleAnimationUsingKeyFrames MakeZoomAnim(bool fireCompleted, Action? cb)
        {
            var a = new DoubleAnimationUsingKeyFrames { FillBehavior = FillBehavior.Stop };
            a.KeyFrames.Add(new EasingDoubleKeyFrame(1.45,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(130)))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
            a.KeyFrames.Add(new EasingDoubleKeyFrame(1.0,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(420)))
                { EasingFunction = new ElasticEase { Oscillations = 1, EasingMode = EasingMode.EaseOut } });
            if (fireCompleted && cb != null)
                a.Completed += (_, _) => cb();
            return a;
        }

        // ── Kollisions-Animation ──────────────────────────────────────────

        private void BeginCollisionAnimation()
        {
            Debug.WriteLine("[DiceAnimation] Collision started");

            int done = 0;

            void MoveAndLock(TranslateTransform trans, double targetX)
            {
                var anim = new DoubleAnimation(0, targetX, TimeSpan.FromMilliseconds(340))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
                    FillBehavior   = FillBehavior.Stop
                };
                anim.Completed += (_, _) => Dispatcher.Invoke(() =>
                {
                    // Basis-Wert setzen damit Y-Shake von korrektem X aus startet
                    trans.BeginAnimation(TranslateTransform.XProperty, null);
                    trans.X = targetX;
                    done++;
                    if (done == 2) OnCollisionReached();
                });
                trans.BeginAnimation(TranslateTransform.XProperty, anim);
            }

            MoveAndLock(PlayerDiceTranslate, +CollisionDisplacement);
            MoveAndLock(EnemyDiceTranslate,  -CollisionDisplacement);
        }

        private void OnCollisionReached()
        {
            Debug.WriteLine("[DiceAnimation] Collision reached");
            BeginImpactAnimation(OnCollisionComplete);
        }

        // ── Impact-Flash (beide Würfel kurz vergrößern) ───────────────────

        private void BeginImpactAnimation(Action onComplete)
        {
            DoubleAnimationUsingKeyFrames MakeImpact()
            {
                var a = new DoubleAnimationUsingKeyFrames { FillBehavior = FillBehavior.Stop };
                a.KeyFrames.Add(new EasingDoubleKeyFrame(1.38,
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80)))
                    { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                a.KeyFrames.Add(new EasingDoubleKeyFrame(1.0,
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(220)))
                    { EasingFunction = new ElasticEase { Oscillations = 2, EasingMode = EasingMode.EaseOut } });
                return a;
            }

            void ApplyImpact(ScaleTransform s, bool trigger)
            {
                var ax = MakeImpact();
                var ay = MakeImpact();
                if (trigger) ay.Completed += (_, _) => Dispatcher.Invoke(onComplete);
                s.BeginAnimation(ScaleTransform.ScaleXProperty, ax);
                s.BeginAnimation(ScaleTransform.ScaleYProperty, ay);
            }

            ApplyImpact(PlayerDiceScale, false);
            ApplyImpact(EnemyDiceScale,  true);
        }

        // ── Bestimmen, wer zerbricht ──────────────────────────────────────

        private void OnCollisionComplete()
        {
            var winner = _vm.DiceWinner;

            if (winner == WinningSide.Player)
            {
                Debug.WriteLine("[DiceAnimation] Enemy dice breaks");
                DiceCompareLabel.Text = _vm.DiceResultText;
                PlayBreakAnimation(EnemyDiceImage, EnemyDiceTranslate, OnBreakComplete);
            }
            else if (winner == WinningSide.Enemy)
            {
                Debug.WriteLine("[DiceAnimation] Player dice breaks");
                DiceCompareLabel.Text = _vm.DiceResultText;
                PlayBreakAnimation(PlayerDiceImage, PlayerDiceTranslate, OnBreakComplete);
            }
            else
            {
                Debug.WriteLine("[DiceAnimation] Tie – no dice breaks");
                DiceCompareLabel.Text = _vm.DiceResultText;
                OnBreakComplete();
            }
        }

        // ── Break-Animation ───────────────────────────────────────────────

        private void PlayBreakAnimation(Image diceImage, TranslateTransform diceTrans, Action onComplete)
        {
            // Y-Shake (X liegt bereits auf Kollisionsposition → nicht anfassen)
            var shakeAnim = new DoubleAnimationUsingKeyFrames { FillBehavior = FillBehavior.Stop };
            shakeAnim.KeyFrames.Add(new EasingDoubleKeyFrame(-9,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(55))));
            shakeAnim.KeyFrames.Add(new EasingDoubleKeyFrame(9,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(110))));
            shakeAnim.KeyFrames.Add(new EasingDoubleKeyFrame(-6,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(165))));
            shakeAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(220))));
            shakeAnim.Completed += (_, _) =>
                Dispatcher.Invoke(() => RunBreakFrames(diceImage, onComplete));
            diceTrans.BeginAnimation(TranslateTransform.YProperty, shakeAnim);
        }

        private void RunBreakFrames(Image diceImage, Action onComplete)
        {
            int frameIdx = 0;
            ShowDiceFromPath(diceImage, DiceBreakFramePaths[frameIdx++]);

            var breakTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(140) };
            breakTimer.Tick += (_, _) =>
            {
                if (frameIdx < DiceBreakFramePaths.Length)
                {
                    ShowDiceFromPath(diceImage, DiceBreakFramePaths[frameIdx++]);
                }
                else
                {
                    breakTimer.Stop();
                    diceImage.Source = null;
                    onComplete();
                }
            };
            breakTimer.Start();
        }

        private void OnBreakComplete()
        {
            Debug.WriteLine("[DiceAnimation] Finished");
            _animationRunning = false;
            _vm.OnDiceBattleAnimationComplete();
        }

        // ── Würfelbild-Helfer ─────────────────────────────────────────────

        private void ShowDiceFrame(Image img, int index) =>
            ShowDiceFromPath(img, DiceFramePaths[Math.Clamp(index, 0, 19)]);

        private void ShowDiceFromPath(Image img, string path)
        {
            var bitmap = LoadBitmap(path);
            if (bitmap != null) img.Source = bitmap;
        }

        private static BitmapImage? LoadBitmap(string path) =>
            _spriteCache.GetOrAdd(path, static p =>
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource   = new Uri("pack://application:,,," + p);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
                catch { return null; }
            });

        // ═════════════════════════════════════════════════════════════════
        //  Prozeduraler Sprite-Fallback
        // ═════════════════════════════════════════════════════════════════

        private void BuildSpider()
        {
            var c = EnemySpriteCanvas;
            c.Children.Add(MakeEllipse(20, 40, 56, 42, "#1A1A1A"));
            c.Children.Add(MakeEllipse(32, 20, 34, 28, "#222244"));
            c.Children.Add(MakeEllipse(38, 28, 6, 6, "#FF3333"));
            c.Children.Add(MakeEllipse(54, 28, 6, 6, "#FF3333"));
            c.Children.Add(MakePath("M20,50 L0,30",   "#444466", 1.0, true, 4));
            c.Children.Add(MakePath("M24,58 L0,58",   "#444466", 1.0, true, 4));
            c.Children.Add(MakePath("M26,68 L4,86",   "#444466", 1.0, true, 4));
            c.Children.Add(MakePath("M76,50 L96,30",  "#444466", 1.0, true, 4));
            c.Children.Add(MakePath("M72,58 L96,58",  "#444466", 1.0, true, 4));
            c.Children.Add(MakePath("M70,68 L92,86",  "#444466", 1.0, true, 4));
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

        private void BuildOgre()
        {
            var c = EnemySpriteCanvas;
            c.Children.Add(MakeEllipse(20, 0, 70, 56, "#7A5A2A"));
            c.Children.Add(MakeEllipse(30, 14, 16, 14, "#CC2200"));
            c.Children.Add(MakeEllipse(64, 14, 16, 14, "#CC2200"));
            c.Children.Add(MakeEllipse(35, 17, 7, 7, "#111111"));
            c.Children.Add(MakeEllipse(69, 17, 7, 7, "#111111"));
            c.Children.Add(Rect(8, 54, 94, 78, "#6A4A1A", rx: 6));
            c.Children.Add(MakeEllipse(-4, 48, 30, 24, "#7A5A2A"));
            c.Children.Add(MakeEllipse(96, 48, 30, 24, "#7A5A2A"));
            c.Children.Add(Rect(-8, 58, 22, 54, "#7A5A2A", rx: 5));
            c.Children.Add(Rect(96, 58, 22, 54, "#7A5A2A", rx: 5));
            c.Children.Add(Rect(16, 130, 34, 44, "#5A3A0A", rx: 4));
            c.Children.Add(Rect(60, 130, 34, 44, "#5A3A0A", rx: 4));
            c.Children.Add(Rect(108, 18, 14, 78, "#888888", rx: 4));
            c.Children.Add(MakeEllipse(102, 4, 26, 24, "#999999"));
            c.Children.Add(MakePath("M34,60 L52,72", "#3A2A0A", 0.8, isStroke: true, sw: 2.5));
        }

        private void BuildDragon()
        {
            var c = EnemySpriteCanvas;
            c.Children.Add(MakePath("M0,70 L44,4 L64,44 L44,68 Z",   "#880022", 0.9));
            c.Children.Add(MakePath("M160,70 L116,4 L96,44 L116,68 Z", "#880022", 0.9));
            c.Children.Add(MakeEllipse(44, 56, 80, 90, "#AA0033"));
            c.Children.Add(Rect(62, 28, 40, 36, "#AA0033", rx: 10));
            c.Children.Add(MakeEllipse(46, 4, 78, 52, "#CC0044"));
            c.Children.Add(MakePath("M52,4 L44,0 L60,20 Z",   "#550011", 1.0));
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
            c.Children.Add(MakePath("M44,158 L50,140 L58,158 Z",  "#AA0033", 1.0));
            c.Children.Add(MakePath("M92,158 L100,140 L108,158 Z", "#AA0033", 1.0));
        }

        // ═════════════════════════════════════════════════════════════════
        //  Shape helpers
        // ═════════════════════════════════════════════════════════════════

        private static UIElement Rect(double l, double t, double w, double h,
            string fill, double rx = 0, double opacity = 1.0)
        {
            var r = new Rectangle
            {
                Width = w, Height = h,
                Fill = Brush(fill), RadiusX = rx, RadiusY = rx, Opacity = opacity
            };
            Set(r, l, t); return r;
        }

        private static UIElement MakeEllipse(double l, double t, double w, double h, string fill)
        {
            var e = new Ellipse { Width = w, Height = h, Fill = Brush(fill) };
            Set(e, l, t); return e;
        }

        private static UIElement MakeEllipseWithStroke(double l, double t, double w, double h,
            string fill, string? stroke, double sw)
        {
            var e = new Ellipse { Width = w, Height = h, Fill = Brush(fill) };
            if (stroke != null) { e.Stroke = Brush(stroke); e.StrokeThickness = sw; }
            Set(e, l, t); return e;
        }

        private static UIElement MakePath(string data, string fill, double opacity,
            bool isStroke = false, double sw = 1)
        {
            var p = new System.Windows.Shapes.Path
            {
                Data    = Geometry.Parse(data),
                Fill    = isStroke ? Brushes.Transparent : Brush(fill),
                Opacity = opacity
            };
            if (isStroke) { p.Stroke = Brush(fill); p.StrokeThickness = sw; }
            return p;
        }

        private static SolidColorBrush Brush(string hex) =>
            new((Color)ColorConverter.ConvertFromString(hex));

        private static void Set(UIElement e, double l, double t)
        {
            Canvas.SetLeft(e, l);
            Canvas.SetTop(e, t);
        }

        // ═════════════════════════════════════════════════════════════════
        //  ViewModel observer
        // ═════════════════════════════════════════════════════════════════

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
            if (!_vm.IsGameOver) e.Cancel = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
