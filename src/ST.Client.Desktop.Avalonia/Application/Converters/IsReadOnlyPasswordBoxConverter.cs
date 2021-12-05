using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FluentAvalonia.UI.Media;
using System;
using System.Globalization;

namespace System.Application.Converters
{
    public class IsReadOnlyPasswordBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                if (parameter is string p && p.Equals("invert", StringComparison.OrdinalIgnoreCase))
                {
                    b = !b;
                }

                if (b)
                    return '•';
            }
            return default(char);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
