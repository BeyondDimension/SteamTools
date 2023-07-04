namespace BD.WTTS.Converters;

public class StringEqualsConverter : IValueConverter
{
    static string ToString(object? obj) => obj?.ToString() ?? string.Empty;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var value_ = ToString(value);
        var parameter_ = ToString(parameter);
        return value_ == parameter_;
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