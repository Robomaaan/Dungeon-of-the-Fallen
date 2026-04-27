namespace Projektarbeit_Dungeon_of_the_Fallen.ViewModels
{
    /// <summary>
    /// Represents a visual object on the dungeon canvas.
    /// Used for sprite-based rendering in 2.5D pseudo-isometric view.
    /// </summary>
    public class RenderObjectViewModel : ViewModelBase
    {
        private string _assetPath = string.Empty;
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private int _zIndex;
        private double _opacity = 1.0;
        private string _debugName = string.Empty;
        private bool _isAnimated;
        private int _frameIndex;
        private string _layer = "default";

        /// <summary>
        /// WPF Resource path to the asset image (e.g., "/Assets/Tiles/StoneDungeon/Floors/floor_stone_01.png")
        /// </summary>
        public string AssetPath
        {
            get => _assetPath;
            set => SetProperty(ref _assetPath, value);
        }

        /// <summary>
        /// Screen X coordinate (Canvas.Left)
        /// </summary>
        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        /// <summary>
        /// Screen Y coordinate (Canvas.Top)
        /// </summary>
        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        /// <summary>
        /// Rendered width in pixels
        /// </summary>
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        /// <summary>
        /// Rendered height in pixels
        /// </summary>
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        /// <summary>
        /// Z-order (Panel.ZIndex) for proper depth sorting.
        /// Higher values appear on top.
        /// </summary>
        public int ZIndex
        {
            get => _zIndex;
            set => SetProperty(ref _zIndex, value);
        }

        /// <summary>
        /// Opacity (0.0 - 1.0)
        /// </summary>
        public double Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, Math.Clamp(value, 0.0, 1.0));
        }

        /// <summary>
        /// Debug/display name for logging
        /// </summary>
        public string DebugName
        {
            get => _debugName;
            set => SetProperty(ref _debugName, value);
        }

        /// <summary>
        /// Whether this object has animation frames
        /// </summary>
        public bool IsAnimated
        {
            get => _isAnimated;
            set => SetProperty(ref _isAnimated, value);
        }

        /// <summary>
        /// Current frame index (for animations)
        /// </summary>
        public int FrameIndex
        {
            get => _frameIndex;
            set => SetProperty(ref _frameIndex, value);
        }

        /// <summary>
        /// Layer identifier (e.g., "floor", "wall", "entity", "effect")
        /// </summary>
        public string Layer
        {
            get => _layer;
            set => SetProperty(ref _layer, value);
        }

        public RenderObjectViewModel()
        {
        }

        public RenderObjectViewModel(
            string assetPath,
            double x,
            double y,
            double width,
            double height,
            int zIndex,
            string debugName,
            string layer = "default",
            double opacity = 1.0)
        {
            AssetPath = assetPath;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            ZIndex = zIndex;
            DebugName = debugName;
            Layer = layer;
            Opacity = opacity;
        }

        public override string ToString() => $"RenderObject({DebugName} @ {X:F0},{Y:F0} Z:{ZIndex} {Layer})";
    }
}
