using System.Globalization;
using Avalonia.Data.Converters;

namespace System.Application.Converters
{
    public class DateFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string para && !string.IsNullOrEmpty(para)) 
            {
                if (value is DateTime date && date != null)
                {
                    return date.ToString(para);
                }
                if (value is DateTimeOffset dateTimeOffset && dateTimeOffset != null)
                {
                    return dateTimeOffset.ToString(para);
                }
                if (value is TimeSpan time && time != null)
                {
                    return time.ToString(para);
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
