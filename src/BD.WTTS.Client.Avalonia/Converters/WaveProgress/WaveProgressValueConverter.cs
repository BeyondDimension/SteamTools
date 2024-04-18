namespace BD.WTTS.Converters;

public sealed class WaveProgressValueConverter : IValueConverter
{
    public static readonly WaveProgressValueConverter Instance = new();

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double i) return 0;
        return 155 - (i * 2.1);
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}

public class WaveProgressValueColorConverter : IValueConverter
{
    public static readonly WaveProgressValueColorConverter Instance = new();

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double d) return Brushes.Black;
        if (d > 50) return Brushes.GhostWhite;
        return Application.Current?.ActualThemeVariant == ThemeVariant.Dark ? Brushes.GhostWhite : Brushes.Black;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}

public class WaveProgressValueTextConverter : IValueConverter
{
    public static readonly WaveProgressValueTextConverter Instance = new();

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not double d ? "0%" : $"{d:#0}%";
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}

public class WaveProgressGradientOffsetConverter : IValueConverter
{
    public static readonly WaveProgressGradientOffsetConverter Instance = new();

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double v)
            return Brushes.Blue;

        var app = Application.Current;
        if (app == null)
            return null;

        Color primaryColor = Colors.DodgerBlue;
        Color accentColor = Colors.Transparent;

        try
        {
            if (app.FindResource("SystemAccentColor") is Color primaryColor2)
            {
                primaryColor = primaryColor2;
            }
            if (app.FindResource("SystemAccentColorLight2") is Color accentColor2)
            {
                accentColor = accentColor2;
            }
        }
        catch { }

        var isLight = app.RequestedThemeVariant == ThemeVariant.Light;

        v /= 100;
        v += isLight ? 0.2 : 0.4;
        if (v > 1)
            v = 1;

        return new LinearGradientBrush()
        {
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            GradientStops = new GradientStops()
            {
                new GradientStop() { Color = primaryColor, Offset = 0 },
                new GradientStop() { Color = isLight ? Colors.Transparent : accentColor, Offset = v },
            },
        };
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}