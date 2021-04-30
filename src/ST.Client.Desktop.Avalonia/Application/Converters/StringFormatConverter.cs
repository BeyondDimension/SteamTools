using System.Globalization;
using Avalonia.Data.Converters;

namespace System.Application.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && parameter is string para && !string.IsNullOrEmpty(para))
            {
                return str.Format(para);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
