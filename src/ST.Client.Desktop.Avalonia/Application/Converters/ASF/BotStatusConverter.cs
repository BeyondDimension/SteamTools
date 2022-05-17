using System.Application.UI.Resx;
using System.Globalization;
using System.Linq;
using ArchiSteamFarm.Steam;
using static ArchiSteamFarm.Core.ASF;

namespace System.Application.Converters
{
    public class BotStatusConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is Bot bot && bot != null)
            {
                if (!bot.KeepRunning &&
                    bot.RequiredInput != EUserInputType.None)
                {
                    var msg = bot.RequiredInput switch
                    {
                        EUserInputType.TwoFactorAuthentication => AppResources.ASF_RequiredInput_TwoFactorAuthentication,
                        EUserInputType.SteamGuard => AppResources.ASF_RequiredInput_TwoFactorAuthentication,
                        EUserInputType.Login => AppResources.ASF_RequiredInput_Password,
                        EUserInputType.Password => AppResources.ASF_RequiredInput_Password,
                        EUserInputType.SteamParentalCode => AppResources.ASF_RequiredInput_SteamParentalCode,
                        _ => string.Empty,
                    };
                    return AppResources.ASF_RequiredInput + msg;
                }
                if (!bot.KeepRunning)
                    return AppResources.ASF_Disable;
                if (!bot.IsConnectedAndLoggedOn)
                    return AppResources.ASF_Offline;
                if (bot.CardsFarmer.Paused ||
                    bot.CardsFarmer.TimeRemaining == TimeSpan.Zero)
                    return AppResources.ASF_Online;
                if (!bot.CardsFarmer.CurrentGamesFarmingReadOnly.Any())
                    return AppResources.ASF_Online;
                return AppResources.ASF_NowCardFarming;
            }
            return value;
        }
    }
}
