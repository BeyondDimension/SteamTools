using System.Application.Services;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Platform;

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
                return IDesktopAvaloniaAppService.Instance.Current.FindResource(key);
            }
            return BindingOperations.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}