using System.Globalization;
using Avalonia.Data.Converters;

namespace System.Application.Converters
{
    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string p)
            {
                if (p.ToLowerInvariant().Equals("invert"))
                {
                    if (value is string str)
                    {
                        return !string.IsNullOrEmpty(str);
                    }
                    return value != null;
                }
            }

            if (value is string v)
            {
                return string.IsNullOrEmpty(v);
            }
            return value is null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}