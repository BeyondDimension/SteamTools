using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Platform;
using System.Application.Services;
using System.Application.UI;
using System.Globalization;

namespace System.Application.Converters
{
    public class DrawingKeyValueConverter : IValueConverter
    {

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is string key)
            {
                return IAvaloniaApplication.Instance.Current.FindResource(key);
            }
            return BindingOperations.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}