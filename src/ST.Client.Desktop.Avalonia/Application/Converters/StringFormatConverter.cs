using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace System.Application.Converters
{
    public class StringFormatConverter : IValueConverter, IMultiValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string str) str = value?.ToString() ?? string.Empty;
            if (parameter is not string para) para = parameter?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(para))
            {
                return str.Format(para);
            }
            return str;
        }

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var stringValues = values.Select(x => (x is not string str) ? x?.ToString() ?? string.Empty : str);
            var format = stringValues.FirstOrDefault() ?? string.Empty;
            var args = stringValues.Skip(1).ToArray();
            return format.Format(args);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
