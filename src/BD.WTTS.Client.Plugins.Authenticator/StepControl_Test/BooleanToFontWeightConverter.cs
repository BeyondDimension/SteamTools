using System;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BD.WTTS.StepControls_Test;

public class BooleanToFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            return Avalonia.Media.FontWeight.Bold;
        }
        else
        {
            return Avalonia.Media.FontWeight.Normal;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}