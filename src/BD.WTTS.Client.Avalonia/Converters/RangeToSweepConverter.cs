namespace BD.WTTS.Converters;

public sealed class RangeToSweepConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        double min = 0d, max = 100d, val = 0d;
        if (values[0] is double value)
        {
            val = value;
        }
        if (values.Count >= 2 && values[1] is double minimum)
        {
            min = minimum;
        }
        if (values.Count >= 3 && values[2] is double maximum)
        {
            max = maximum;
        }
        double m = max - min;
        var result = val / m;
        return result * 360d;
    }
}