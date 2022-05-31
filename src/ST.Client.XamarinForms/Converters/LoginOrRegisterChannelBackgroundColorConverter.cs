using System.Globalization;
#if MAUI
using Color = Microsoft.Maui.Graphics.Color;
#else
using Color = Xamarin.Forms.Color;
#endif

namespace System.Application.Converters;

public class LoginOrRegisterChannelBackgroundColorConverter : IValueConverter
{
    static Color ColorFromHex(string hex)
    {
#if MAUI
        return Color.FromArgb(hex);
#else
        return Color.FromHex(hex);
#endif
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
            _ => ((IBinding)this).DoNothing,
        };
    }
}