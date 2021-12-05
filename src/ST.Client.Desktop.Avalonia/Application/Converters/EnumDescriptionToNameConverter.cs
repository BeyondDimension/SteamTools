using Avalonia.Data.Converters;
using System.Application.UI.Resx;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace System.Application.Converters
{
    public class EnumDescriptionToNameConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (value is Enum e)
            {
                var desc = Enum2.GetDescription(e);
                if (!string.IsNullOrEmpty(desc))
                {
                    if (parameter is not null && 
                        string.Equals(parameter.ToString(), "Localiza", StringComparison.OrdinalIgnoreCase))
                    {
                        return AppResources.ResourceManager.GetString(desc, AppResources.Culture);
                    }
                    return desc;
                }
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
