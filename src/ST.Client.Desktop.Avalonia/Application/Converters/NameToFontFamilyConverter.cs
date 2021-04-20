using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace System.Application.Converters
{
    public class NameToFontFamilyConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (string.IsNullOrEmpty(s) || s.Equals("Default", StringComparison.OrdinalIgnoreCase))
                    return new FontFamily(FontManager.Current.DefaultFontFamilyName);
                return new FontFamily(s);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}