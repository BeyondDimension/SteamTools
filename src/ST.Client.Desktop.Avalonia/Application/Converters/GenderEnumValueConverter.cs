using Avalonia.Data;
using Avalonia.Data.Converters;
using System.Globalization;

namespace System.Application.Converters
{
    public class GenderEnumValueConverter : IValueConverter
    {
        static Gender Convert(object value, CultureInfo culture)
        {
            Gender gender;
            if (value is Gender gender0) gender = gender0;
            else if (value is string str && Enum.TryParse<Gender>(str, true, out var gender1)) gender = gender1;
            else if (value is IConvertible convertible) gender = (Gender)convertible.ToByte(culture);
            else gender = Gender.Unknown;
            return gender;
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            var value2 = Convert(value, culture);
            var parameter2 = Convert(parameter, culture);

            return value2 == parameter2;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return BindingOperations.DoNothing;

            if (value is bool b && b)
            {
                return Convert(parameter, culture);
            }

            return BindingOperations.DoNothing;
        }
    }
}