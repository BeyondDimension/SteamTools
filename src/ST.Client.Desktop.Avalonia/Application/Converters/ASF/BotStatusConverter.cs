using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArchiSteamFarm.Steam;
#if !__MOBILE__
using Avalonia.Data.Converters;
#else
using Xamarin.Forms;
#endif

namespace System.Application.Converters
{
    public class BotStatusConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is Bot bot && bot != null)
            {
                if (!bot.KeepRunning)
                    return "未启用";
                if (!bot.IsConnectedAndLoggedOn)
                    return "离线";
                if (bot.CardsFarmer.Paused ||
                    bot.CardsFarmer.TimeRemaining == TimeSpan.Zero)
                    return "在线";
                if (!bot.CardsFarmer.CurrentGamesFarmingReadOnly.Any())
                    return "在线";
                return "正在挂卡";
            }
            return value;
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture) => throw new NotImplementedException();

    }
}
