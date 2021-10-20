using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Application.Services;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Application.Converters
{
    public class NameToFontFamilyConverter : IValueConverter
    {
        static readonly Lazy<FontFamily> mDefault = new(() => new FontFamily(IPlatformService.Instance.GetDefaultFontFamily()));

        public static FontFamily Default => mDefault.Value;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (string.IsNullOrEmpty(s) || s.Equals("Default", StringComparison.OrdinalIgnoreCase))
                    return Default;
                return FontFamily.Parse(s);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}