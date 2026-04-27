using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Projektarbeit_Dungeon_of_the_Fallen.Converters
{
    public class FallbackImageConverter : IValueConverter
    {
        private const string MissingAssetPath = "/Assets/system/missing_asset.png";

        private static BitmapImage? _missingAssetCache;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (!string.IsNullOrWhiteSpace(path))
            {
                var bitmap = TryLoadResource(path);
                if (bitmap != null)
                    return bitmap;
            }

            return GetMissingAsset();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static BitmapImage? TryLoadResource(string path)
        {
            try
            {
                var uri = new Uri(path, UriKind.Relative);
                var streamInfo = Application.GetResourceStream(uri);
                if (streamInfo == null)
                    return null;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = streamInfo.Stream;
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

        private static BitmapImage? GetMissingAsset()
        {
            if (_missingAssetCache != null)
                return _missingAssetCache;

            _missingAssetCache = TryLoadResource(MissingAssetPath);
            return _missingAssetCache;
        }
    }
}
