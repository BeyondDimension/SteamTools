using SteamKit2;

namespace BD.WTTS.Converters;

public sealed class StringFormatConverter : IValueConverter, IMultiValueConverter
{
    const string Size = "size";
    const string Money = "money";

    static object? Format(object? value, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? Strings.On : Strings.Off;
        if (value is TimeSpan t)
            return t.ToDisplayString();
        if (value is not string str) str = value?.ToString() ?? string.Empty;
        if (parameter is not string para) para = parameter?.ToString() ?? string.Empty;
        if (!string.IsNullOrEmpty(para))
        {
            if (string.Equals(para, Size, StringComparison.OrdinalIgnoreCase)
                && double.TryParse(str, out var num))
            {
                (var length, string unit) = IOPath.GetDisplayFileSize(num);
                if (length == 0)
                    return "0 B";
                return $"{length:###,###.##} {unit}";
            }
            else if (decimal.TryParse(str, out decimal d))
            {
                if (parameter is ECurrencyCode c1)
                {
                    return d.ToString("C", c1.GetCultureInfo());
                }
                else if (parameter is CurrencyCode c2)
                {
                    return d.ToString("C", c2.GetCultureInfo());
                }
                else if (string.Equals(para, Money, StringComparison.OrdinalIgnoreCase))
                {
                    return d.ToString("C", culture);
                }
            }

            var res = Strings.ResourceManager.GetString(para, Strings.Culture);
            if (!string.IsNullOrEmpty(res))
                return res.Format(str);
            return str.Format(para);
        }
        else
        {
            return str.Format(string.Empty);
        }
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Format(value, parameter, culture);
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var format = values.FirstOrDefault()?.ToString() ?? string.Empty;
        if (format == string.Empty || string.Equals("(Unset)", format.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        if (parameter is not null && values.Count >= 2)
        {
            if (string.Equals(parameter.ToString(), Money, StringComparison.OrdinalIgnoreCase))
            {
                return Format(values[0], values[1] ?? Money, culture);
            }
            else
            {
                return Format(values[0], parameter, culture);
            }
        }

        var stringValues = values.Select(x => x is not string str ? x?.ToString() ?? string.Empty : str);
        var args = stringValues.Skip(1).ToArray();
        return format.Format(args);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
