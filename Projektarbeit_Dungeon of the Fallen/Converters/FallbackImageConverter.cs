using System.Collections.Concurrent;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Projektarbeit_Dungeon_of_the_Fallen.Converters
{
    public class FallbackImageConverter : IValueConverter
    {
        private const string MissingAssetPath = "/Assets/system/missing_asset.png";
        private const string DungeonTilePrefix = "/Assets/Tiles/";

        // Jedes Bild wird einmal geladen und dann aus dem Cache zurückgegeben.
        private static readonly ConcurrentDictionary<string, ImageSource?> _cache = new();

        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var path = value as string;
            if (!string.IsNullOrWhiteSpace(path))
            {
                var bitmap = _cache.GetOrAdd(path, LoadPackUri);
                if (bitmap != null)
                    return bitmap;
            }

            return _cache.GetOrAdd(MissingAssetPath, LoadPackUri);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        // Lädt ein Bild über den korrekten WPF Pack-URI (pack://application:,,,/Assets/...).
        // Gibt null zurück wenn das Asset nicht existiert — wird genau einmal pro Pfad aufgerufen.
        private static ImageSource? LoadPackUri(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,," + path);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmap.EndInit();
                bitmap.Freeze();

                if (IsDungeonTile(path))
                    return CropTransparentBorders(bitmap);

                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsDungeonTile(string path) =>
            path.Contains(DungeonTilePrefix, StringComparison.OrdinalIgnoreCase);

        private static ImageSource CropTransparentBorders(BitmapSource source)
        {
            var converted = source.Format == PixelFormats.Bgra32
                ? source
                : new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            var width = converted.PixelWidth;
            var height = converted.PixelHeight;
            var stride = width * 4;
            var pixels = new byte[height * stride];
            converted.CopyPixels(pixels, stride, 0);

            if (converted.CanFreeze && !converted.IsFrozen)
                converted.Freeze();

            var minX = width;
            var minY = height;
            var maxX = -1;
            var maxY = -1;

            for (var y = 0; y < height; y++)
            {
                var rowOffset = y * stride;
                for (var x = 0; x < width; x++)
                {
                    if (pixels[rowOffset + x * 4 + 3] == 0)
                        continue;

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX || maxY < minY)
                return source;

            if (minX == 0 && minY == 0 && maxX == width - 1 && maxY == height - 1)
                return source;

            var cropped = new CroppedBitmap(
                converted,
                new Int32Rect(minX, minY, maxX - minX + 1, maxY - minY + 1));
            cropped.Freeze();
            return cropped;
        }
    }
}
