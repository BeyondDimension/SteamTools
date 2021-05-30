using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if !__MOBILE__
using Avalonia.Data.Converters;
#else
using Xamarin.Forms;
#endif

namespace System.Application.Converters
{
    public class StringFormatConverter : IValueConverter, IMultiValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is not string str) str = value?.ToString() ?? string.Empty;
            if (parameter is not string para) para = parameter?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(para))
            {
                return str.Format(para);
            }
            return str;
        }

        public object? Convert(IList<object?> values, Type? targetType, object? parameter, CultureInfo? culture)
        {
            var stringValues = values.Select(x => (x is not string str) ? x?.ToString() ?? string.Empty : str);
            var format = stringValues.FirstOrDefault() ?? string.Empty;
            var args = stringValues.Skip(1).ToArray();
            return format.Format(args);
        }

        public object? Convert(object?[] values, Type? targetType, object? parameter, CultureInfo? culture)
        {
            IList<object?> values_ = values;
            return Convert(values_, targetType, parameter, culture);
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture) => throw new NotImplementedException();

        public object[]? ConvertBack(object? value, Type[]? targetTypes, object? parameter, CultureInfo? culture) => throw new NotImplementedException();
    }
}
