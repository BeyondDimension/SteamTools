using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System.Application.Converters;

public class LoginOrRegisterChannelTextConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo? culture)
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
        return ((IBinding)this).DoNothing;
    }
}