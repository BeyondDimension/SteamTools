using Avalonia.Data;
using System.Application.Models;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace System.Application.Converters
{
    public class GuidToUrlConverter : ImageValueConverter
    {
        public override object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Guid imageid)
            {
                if (Guid.Empty == imageid)
                {
                    return null;
                }
                return ImageUrlHelper.GetImageApiUrlById(imageid);
            }
            return value;
        }
    }
}