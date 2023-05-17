namespace BD.WTTS.Converters;

public sealed class NameToFontFamilyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            //if (AvaloniaFontManagerImpl.IsDefaultFontFamilyName(s))
            //    return AvaloniaFontManagerImpl.Default;
            //if (s.Equals(IFontManager.KEY_DefaultConsole, StringComparison.OrdinalIgnoreCase))
            //    return AvaloniaFontManagerImpl.DefaultConsole;
            return FontFamily.Parse(s);
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}