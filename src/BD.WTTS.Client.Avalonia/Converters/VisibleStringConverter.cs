namespace BD.WTTS.Converters;

public sealed class VisibleStringConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && values.FirstOrDefault() is bool isVisble)
        {
            if (isVisble)
            {
                return values[1];
            }
            else if (values.Count >= 3)
            {
                return values[2];
            }
            else
            {
                return null;
            }
        }
        return values;
    }

}
