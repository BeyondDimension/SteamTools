using Avalonia.Data.Converters;
using System.Collections.Generic;
using System.Globalization;

namespace System.Application.Converters
{
    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = false;

            if (value is string v)
            {
                result = string.IsNullOrEmpty(v);
            }
            else if (value is int i)
            {
                result = i == 0;
            }
            else if (value is decimal d)
            {
                result = d == 0;
            }
            else
            {
                result = value is null;
            }

            if (parameter is string p)
            {
                if (p.ToLowerInvariant().Equals("invert"))
                {
                    result = !result;
                }
            }
            return result;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string))
            {
                return value.ToString();
            }
            return value;
        }
    }
}