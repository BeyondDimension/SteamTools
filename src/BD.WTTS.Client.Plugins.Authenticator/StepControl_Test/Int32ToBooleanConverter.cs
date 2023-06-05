using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BD.WTTS.StepControls_Test;

public class Int32ToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int intvalue && intvalue > 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}