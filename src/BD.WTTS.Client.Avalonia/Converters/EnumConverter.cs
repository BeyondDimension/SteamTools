namespace BD.WTTS.Converters;

public sealed class EnumConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
        {
            return value;
        }
        if (parameter is Type type && type.IsEnum)
        {
            if (value.GetType() == type)
            {
                return value;
            }
            else if (value is Enum enumValue)
            {
                return Enum.ToObject(type, enumValue);
            }
            else if (value is string strValue && Enum.TryParse(type, strValue, true, out var val))
            {
                return val;
            }
            else if (value is int intValue)
            {
                return Enum.ToObject(type, intValue);
            }
            else if (value is short shortValue)
            {
                return Enum.ToObject(type, shortValue);
            }
            else if (value is byte byteValue)
            {
                return Enum.ToObject(type, byteValue);
            }
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}