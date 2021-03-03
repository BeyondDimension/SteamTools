using System.Globalization;
using Avalonia.Data.Converters;

namespace System.Application.Converters
{
    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                var k = (string) parameter;
                if (k.ToLowerInvariant().Contains("invert"))
                    return value != null;
            }

            return value is null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}