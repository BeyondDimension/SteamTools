using System.Globalization;
using Xamarin.Forms;
using XFSPathGeometryConverter = Xamarin.Forms.Shapes.PathGeometryConverter;

namespace System.Application.Converters
{
    public class PathGeometryConverter : XFSPathGeometryConverter, IValueConverter
    {
        internal static object? GetGeometryByPathString(XFSPathGeometryConverter converter, string pathStr)
        {
            try
            {
                // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/shapes/path-markup-syntax
                var pathData = converter.ConvertFromInvariantString(pathStr);
                return pathData;
            }
            catch
            {
            }
            return Binding.DoNothing;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var valueStr = value?.ToString();
            if (!string.IsNullOrWhiteSpace(valueStr))
            {
                var r = GetGeometryByPathString(this, valueStr);
                return r;
            }
            return Binding.DoNothing;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}