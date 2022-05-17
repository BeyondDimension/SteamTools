using Avalonia.Controls;
using System.Application.UI;
using System.Globalization;

namespace System.Application.Converters
{
    public class DrawingKeyValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (value is string key)
                return IAvaloniaApplication.Instance.Current.FindResource(key);
            return ((IBinding)this).DoNothing;
        }
    }
}