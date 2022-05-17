using System.Globalization;

namespace System.Application.Converters
{
    public class GuidToUrlConverter : ImageValueConverter
    {
        public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Guid imageid)
            {
                if (imageid == Guid.Empty)
                {
                    return null;
                }
                return ImageUrlHelper.GetImageApiUrlById(imageid);
            }
            return value;
        }
    }
}