using System.Globalization;
#if MAUI
using XFApplication = Microsoft.Maui.Controls.Application;
#else
using Xamarin.Forms;
using XFApplication = Xamarin.Forms.Application;
#endif

namespace System.Application.Converters;

public class ResourceKeyConverter : IValueConverter
{
    const string DrawingSvg = "DrawingSvg";

    internal static object? GetResourceByKey(string key)
    {
        var res = XFApplication.Current!.Resources;
        if (res.ContainsKey(DrawingSvg))
        {
            var svgRes = res[DrawingSvg];
            if (svgRes is ResourceDictionary drawingSvg)
            {
                switch (key)
                {
                    case "Steam":
                        key = "SteamDrawing";
                        break;
                    case "PhoneNumber":
                        key = "Phone";
                        break;
                }
                if (drawingSvg.ContainsKey(key))
                {
                    return drawingSvg[key];
                }
            }
        }
        return null;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var valueStr = value?.ToString();
        if (!string.IsNullOrWhiteSpace(valueStr))
        {
            var r = GetResourceByKey(valueStr);
            return r;
        }
        return ((IBinding)this).DoNothing;
    }
}