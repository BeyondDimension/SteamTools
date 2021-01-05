using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MetroTrilithon.UI.Converters
{
	public class UniversalBooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Visibility result;
			var bValue = false;
			if (value is bool)
			{
				bValue = (bool)value;
			}

			if (bValue)
			{
				result = Visibility.Visible; // true に対応する Visibility
				var pValue = parameter as string;
				var p = pValue?.Split(':');
				if (p?.Length >= 1)
				{
					// 最初のパラメーターに Visible 以外が設定されていたら、true に対応する Visibility を上書き
					if (CompareString(p[0], "Hidden")) result = Visibility.Hidden;
					else if (CompareString(p[0], "Collapsed")) result = Visibility.Collapsed;
				}
			}
			else
			{
				result = Visibility.Collapsed; // false に対応する Visibility
				var pValue = parameter as string;
				var p = pValue?.Split(':');
				if (p?.Length >= 2)
				{
					// 2 番目のパラメーターに Collapsed 以外が設定されていたら、false に対応する Visibility を上書き
					if (CompareString(p[1], "Visible")) result = Visibility.Visible;
					else if (CompareString(p[1], "Hidden")) result = Visibility.Hidden;
				}
			}

			return result;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}


		private static bool CompareString(string strA, string strB)
		{
			return string.Compare(strA, strB, StringComparison.InvariantCultureIgnoreCase) == 0;
		}
	}
}
