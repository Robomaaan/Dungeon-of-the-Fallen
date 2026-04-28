using System.Collections.Concurrent;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Projektarbeit_Dungeon_of_the_Fallen.Converters
{
    public class FallbackImageConverter : IValueConverter
    {
        private const string MissingAssetPath = "/Assets/system/missing_asset.png";

        // Jedes Bild wird einmal geladen und dann aus dem Cache zurückgegeben.
        private static readonly ConcurrentDictionary<string, BitmapImage?> _cache = new();

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
        private static BitmapImage? LoadPackUri(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,," + path);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
