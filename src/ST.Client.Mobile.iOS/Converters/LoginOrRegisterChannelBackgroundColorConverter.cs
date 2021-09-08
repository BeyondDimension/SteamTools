using System.Globalization;
#if !__MOBILE__
using Avalonia.Data.Converters;
#else
using Xamarin.Forms;
#endif

namespace System.Application.Converters
{
    public class LoginOrRegisterChannelBackgroundColorConverter : IValueConverter
    {
        static Color ColorFromHex(string hex)
        {
            return Color.FromHex(hex);
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var valueStr = value?.ToString();
            return valueStr switch
            {
                nameof(FastLoginChannel.Steam) => ColorFromHex("#145c8f"),
                nameof(FastLoginChannel.Xbox) or nameof(FastLoginChannel.Microsoft) => ColorFromHex("#027d00"),
                nameof(FastLoginChannel.Apple) => ColorFromHex("#000000"),
                nameof(FastLoginChannel.QQ) => ColorFromHex("#12B7F5"),
                "Phone" or "PhoneNumber" => ColorFromHex("#2196F3"),
                _ => Binding.DoNothing,
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}