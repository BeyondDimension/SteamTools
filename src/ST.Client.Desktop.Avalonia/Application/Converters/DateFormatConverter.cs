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
                if (value is DateTime date)
                {
                    return date.ToString(para);
                }
                if (value is DateTimeOffset dateTimeOffset)
                {
                    return dateTimeOffset.ToString(para);
                }
                if (value is TimeSpan time)
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
