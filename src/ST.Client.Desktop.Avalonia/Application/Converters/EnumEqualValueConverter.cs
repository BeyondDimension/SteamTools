using Avalonia.Data;
using Avalonia.Data.Converters;
using System.Globalization;

namespace System.Application.Converters
{
    public class EnumEqualValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            if (value is Enum)
            {
                if (parameter is Enum)
                {
                    return value.Equals(parameter);
                }
                else if (parameter is string)
                {
                    return value.ToString() == parameter.ToString();
                }
            }
            return false;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return BindingOperations.DoNothing;

            if (value is Enum)
            {
                return value;
            }

            if (value is bool b && b && parameter is Enum)
            {
                return parameter;
            }

            return BindingOperations.DoNothing;
        }
    }
}