using Avalonia.Media;
using System.Application.Services.Implementation;
using System.Globalization;

namespace System.Application.Converters
{
    public class NameToFontFamilyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (AvaloniaFontManagerImpl.IsDefaultFontFamilyName(s))
                    return AvaloniaFontManagerImpl.Default;
                //if (s.Equals(IFontManager.KEY_DefaultConsole, StringComparison.OrdinalIgnoreCase))
                //    return AvaloniaFontManagerImpl.DefaultConsole;
                return FontFamily.Parse(s);
            }
            return null;
        }
    }
}