using System.Globalization;
using Avalonia.Data.Converters;

namespace System.Application.Converters
{
    public class SizeConverter : IValueConverter
    {
        //private const int SizeUnit = 1024;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out double b))
            {
                (var length, string unit) = IOPath.GetSize(b);
                return $"{length:###,###.##} {unit}";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}