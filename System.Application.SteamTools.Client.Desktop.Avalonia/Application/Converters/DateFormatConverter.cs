using System.Globalization;
using Avalonia.Data.Converters;

namespace System.Application.Converters
{
    public class DateFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date && date != null && parameter is string para && !string.IsNullOrEmpty(para))
            {
               return date.ToString(para);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
