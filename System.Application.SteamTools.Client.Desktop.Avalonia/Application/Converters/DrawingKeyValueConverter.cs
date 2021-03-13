using System.Application.Services;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
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
                return DI.Get<IDesktopAvaloniaAppService>().CurrentApp.FindResource(key);
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}