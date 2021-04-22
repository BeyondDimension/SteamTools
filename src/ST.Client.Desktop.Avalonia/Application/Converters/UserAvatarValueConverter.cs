using System.Application.Models;
using System.Globalization;

namespace System.Application.Converters
{
    public class UserAvatarValueConverter : ImageValueConverter
    {
        public override object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int width = 96;
            if (parameter is string para)
            {
                if (int.TryParse(para, out int w))
                    width = w;
            }

            if (value is UserInfoDTO userInfo && userInfo.SteamAccountId.HasValue)
            {
                // Steam Avatar
            }

            if (value is IUserDTO user && user.Avatar.HasValue)
            {
                // Guid Avatar
            }

            var uri = GetResUri("/Application/UI/Assets/AppResources/avater_default.png");
            var asset = OpenAssets(uri);
            return GetDecodeBitmap(asset, width);
        }
    }
}