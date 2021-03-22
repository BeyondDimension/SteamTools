using System.Globalization;
using Avalonia.Data.Converters;

namespace System.Application.Converters
{
	public class ReverseBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(value as bool?) ?? false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(value as bool?) ?? false;
		}
	}
}
