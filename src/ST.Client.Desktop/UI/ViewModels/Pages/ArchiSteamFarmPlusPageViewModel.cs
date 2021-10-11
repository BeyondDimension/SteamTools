using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive.Linq;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    partial class ArchiSteamFarmPlusPageViewModel
    {
        public ArchiSteamFarmPlusPageViewModel()
        {

        }

        public async void PauseOrResumeBotFarming(Bot bot)
        {
            (bool success, string message) r;

            if (bot.CardsFarmer.Paused)
            {
                r = bot.Actions.Resume();
            }
            else
            {
                r = await bot.Actions.Pause(true).ConfigureAwait(false);
            }
            var message = r.success ? r.message : string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, r.message);

            Toast.Show("<" + bot.BotName + "> " + message);
        }
    }
}
