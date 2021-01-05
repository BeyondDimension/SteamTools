using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MetroTrilithon.UI.Converters
{
	/// <summary>
	/// 文字列が null または空文字のときに <see cref="Visibility.Collapsed"/>、それ以外のときに <see cref="Visibility.Visible"/> を返すコンバーターを定義します。
	/// </summary>
	public class StringToVisiblityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (parameter is bool p && p)
			{
				return !string.IsNullOrEmpty(value as string)
	? Visibility.Collapsed
	: Visibility.Visible;
			}
			return !string.IsNullOrEmpty(value as string)
				? Visibility.Visible
				: Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
