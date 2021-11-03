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
    public class LoginOrRegisterChannelTextConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?>? values, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (values != null && values.Count >= 3)
            {
                var stringValues = values.Select(x => (x is not string str) ? x?.ToString() ?? string.Empty : str).ToArray();
                var args = stringValues.Skip(1).FirstOrDefault();
                switch (args)
                {
                    case "Xbox":
                        args = "Xbox Live";
                        break;
                    case "Phone" or "PhoneNumber":
                        return stringValues[2];
                }
                var format = stringValues.FirstOrDefault() ?? string.Empty;
                return format.Format(args);
            }
            return Binding.DoNothing;
        }

        public object? Convert(object?[]? values, Type? targetType, object? parameter, CultureInfo? culture)
        {
            IList<object?>? values_ = values;
            return Convert(values_, targetType, parameter, culture);
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture) => throw new NotImplementedException();

        public object[]? ConvertBack(object? value, Type[]? targetTypes, object? parameter, CultureInfo? culture) => throw new NotImplementedException();
    }
}