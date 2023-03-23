namespace BD.WTTS.Converters;

public sealed class IsNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool result;
        if (value is string v)
        {
            result = string.IsNullOrEmpty(v);
        }
        else if (value is int i)
        {
            result = i == 0;
        }
        else if (value is decimal d)
        {
            result = d == 0;
        }
        else
        {
            result = value is null;
        }
        if (parameter is string p && p.Equals("invert", StringComparison.OrdinalIgnoreCase))
        {
            result = !result;
        }
        return result;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(string))
        {
            return value?.ToString();
        }
        return value;
    }
}