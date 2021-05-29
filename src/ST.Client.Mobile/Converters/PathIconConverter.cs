using System.Globalization;
using Xamarin.Forms;
using static System.Application.Converters.PathGeometryConverter;
using static System.Application.Converters.ResourceKeyConverter;
using XFSPathGeometryConverter = Xamarin.Forms.Shapes.PathGeometryConverter;

namespace System.Application.Converters
{
    public class PathIconConverter : XFSPathGeometryConverter, IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var valueStr = value?.ToString();
            if (!string.IsNullOrWhiteSpace(valueStr))
            {
                var pathStr = GetResourceByKey(valueStr)?.ToString();
                if (!string.IsNullOrWhiteSpace(pathStr))
                {
                    var r = GetGeometryByPathString(this, pathStr);
                    return r;
                }
            }
            return Binding.DoNothing;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}