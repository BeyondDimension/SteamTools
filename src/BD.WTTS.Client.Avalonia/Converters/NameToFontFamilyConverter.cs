namespace BD.WTTS.Converters;

public sealed class NameToFontFamilyConverter : IValueConverter
{
    static FontFamily GetDefault()
    {
        try
        {
            var fontFamily = IPlatformService.Instance.GetDefaultFontFamily();
            return FontFamily.Parse(fontFamily);
        }
        catch
        {
        }
        return FontFamily.Default;
    }

    static readonly Lazy<FontFamily> _FontFamily = new(GetDefault);

    public static FontFamily DefaultFontFamily => _FontFamily.Value;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            //if (AvaloniaFontManagerImpl.IsDefaultFontFamilyName(s))
            //    return AvaloniaFontManagerImpl.Default;
            //if (s.Equals(IFontManager.KEY_DefaultConsole, StringComparison.OrdinalIgnoreCase))
            //    return AvaloniaFontManagerImpl.DefaultConsole;
            try
            {
                return FontFamily.Parse(s);
            }
            catch
            {

            }
        }
        return DefaultFontFamily;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}