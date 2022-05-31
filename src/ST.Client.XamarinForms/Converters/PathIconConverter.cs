using System.Globalization;
using static System.Application.Converters.PathGeometryConverter;
using static System.Application.Converters.ResourceKeyConverter;
#if MAUI
using XFSPathGeometryConverter = Microsoft.Maui.Controls.Shapes.PathGeometryConverter;
#else
using XFSPathGeometryConverter = Xamarin.Forms.Shapes.PathGeometryConverter;
#endif

namespace System.Application.Converters;

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
                var r = GetGeometryByPathString(this, this, pathStr);
                return r;
            }
        }
        return ((IBinding)this).DoNothing;
    }
}