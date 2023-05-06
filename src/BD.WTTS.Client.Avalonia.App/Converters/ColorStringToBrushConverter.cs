namespace BD.WTTS.Converters;

public sealed class ColorStringToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType.IsAssignableFrom(typeof(ISolidColorBrush)))
        {
            if (value is Color c)
                return new SolidColorBrush(c);
            if (value is Color2 c2)
                return new SolidColorBrush(c2);
            if (value is string c3 && Color.TryParse(c3, out var cc))
                return new SolidColorBrush(cc);
        }
        else
        {
            if (value is string c3 && Color.TryParse(c3, out var cc))
                return cc;
            if (value is string c4 && Color2.TryParse(c4, out var cc1))
                return cc1;
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(string))
        {
            if (value is ISolidColorBrush sc)
                return ColorToHex(sc.Color);
            if (value is Color c1)
                return ColorToHex(c1);
            if (value is Color2 c2)
                return ColorToHex(c2);
        }
        else
        {
            if (value is ISolidColorBrush sc)
                return sc.Color;
            if (value is Color || value is Color2)
                return value;
        }
        return this.DoNothing();
    }

    static string ColorToHex(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
}
