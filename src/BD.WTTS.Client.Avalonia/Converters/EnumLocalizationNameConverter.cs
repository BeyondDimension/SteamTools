namespace BD.WTTS.Converters;

public sealed class EnumLocalizationNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return null;
        if (value is Enum e)
        {
            string? text;
            var desc = e.GetDescription();
            if (string.IsNullOrEmpty(desc))
            {
                var type = value.GetType();
                text = Strings.ResourceManager.GetString($"{type.Name}_{Enum.GetName(type, value)}", Strings.Culture);
            }
            else
            {
                text = Strings.ResourceManager.GetString(desc, Strings.Culture);
                if (string.IsNullOrEmpty(text))
                    return desc;
            }

            if (!string.IsNullOrEmpty(text))
                return text;
        }
        return value.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
