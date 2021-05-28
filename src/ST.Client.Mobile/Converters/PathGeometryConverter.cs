using System.Globalization;
using Xamarin.Forms;
using XFSPathGeometryConverter = Xamarin.Forms.Shapes.PathGeometryConverter;

namespace System.Application.Converters
{
    public class PathGeometryConverter : XFSPathGeometryConverter, IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var valueStr = value?.ToString();
            if (!string.IsNullOrWhiteSpace(valueStr))
            {
                try
                {
                    // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/shapes/path-markup-syntax
                    var pathData = ConvertFromInvariantString(valueStr);
                    return pathData;
                }
                catch
                {
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}