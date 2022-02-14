using Android.App;
using Android.Runtime;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using VM = System.Application.UI.ViewModels.ArchiSteamFarmPlusPageViewModel;

namespace System.Application.Services.Native
{
    /// <summary>
    /// 用于 ASF 的代理前台服务
    /// </summary>
    [Register(JavaPackageConstants.Services + TAG)]
    [Service]
    sealed partial class ArchiSteamFarmForegroundService : ForegroundService
    {
        const string TAG = nameof(ArchiSteamFarmForegroundService);

        public override void OnStart()
        {
            base.OnStart();

            VM.StartOrStopASF(startOrStop: true);
        }

        public override void OnStop()
        {
            base.OnStop();

            VM.StartOrStopASF(startOrStop: false);
        }
    }

    partial class ArchiSteamFarmForegroundService
    {
        public override NotificationType NotificationType
              => NotificationType.ArchiSteamFarmForegroundService;

        public override string NotificationText
            => AppResources.ArchiSteamFarmForegroundService_NotificationText;

        public override string? NotificationEntranceAction
            => nameof(TabItemViewModel.TabItemId.ArchiSteamFarmPlus);
    }
}
