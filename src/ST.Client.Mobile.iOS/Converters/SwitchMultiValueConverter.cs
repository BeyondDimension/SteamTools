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
    public class SwitchMultiValueConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?>? values, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (values != null && values.Count > 0)
            {
                var first = values.FirstOrDefault();
                if (first != null)
                {
                    try
                    {
                        var index = System.Convert.ToInt32(first);
                        if (index > 0 && index < values.Count)
                        {
                            return values[index];
                        }
                    }
                    catch
                    {

                    }
                }
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
