using UserService_ = BD.WTTS.Services.UserService; // 与 Android.Content.Context.UserService 冲突

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class Program
{
    static void OnStartup(IApplication.IProgramHost host, bool isTrace = false)
    {
        if (isTrace) StartWatchTrace.Record();

        host.InitVisualStudioAppCenterSDK();
        if (isTrace) StartWatchTrace.Record("AppCenter");

        StartupToastIntercept.OnStartuped();

        if (host.IsMainProcess)
        {
            OnStartupInMainProcessAsyncVoid();
            async void OnStartupInMainProcessAsyncVoid()
            {
                await Task.Run(() =>
                {
                    ActiveUserPost(host, ActiveUserAnonymousStatisticType.OnStartup);
                    if (GeneralSettings.IsAutoCheckUpdate.Value)
                    {
                        IApplicationUpdateService.Instance.CheckUpdate(showIsExistUpdateFalse: false);
                    }
                });
            }

            //INotificationService.ILifeCycle.Instance?.OnStartup();
        }
    }

    static async void ActiveUserPost(IApplication.IStartupArgs args, ActiveUserAnonymousStatisticType type)
    {
        if (!args.IsMainProcess) return;
        try
        {
            var userService = UserService_.Current;
            var isAuthenticated = userService.IsAuthenticated;
            var csc = IMicroServiceClient.Instance;
            if (isAuthenticated)
            {
                // 刷新用户信息
                var rspRUserInfo = await csc.Manage.RefreshUserInfo();
                if (rspRUserInfo.IsSuccess && rspRUserInfo.Content != null)
                {
                    await userService.SaveUserAsync(rspRUserInfo.Content);
                }
            }
#if !__MOBILE__ && !MAUI
            var screens = Host.Instance.App.MainWindow!.Screens;
#else
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            var mainDisplayInfoH = mainDisplayInfo.Height.ToInt32(NumberToInt32Format.Ceiling);
            var mainDisplayInfoW = mainDisplayInfo.Width.ToInt32(NumberToInt32Format.Ceiling);
#endif
            var req = new ActiveUserRecordDTO
            {
                Type = type,
#if __MOBILE__ || MAUI
                ScreenCount = 1,
                PrimaryScreenPixelDensity = mainDisplayInfo.Density,
                PrimaryScreenWidth = mainDisplayInfoW,
                PrimaryScreenHeight = mainDisplayInfoH,
                SumScreenWidth = mainDisplayInfoW,
                SumScreenHeight = mainDisplayInfoH,
#else
                ScreenCount = screens.ScreenCount,
                PrimaryScreenPixelDensity = screens.Primary?.Scaling ?? default,
                PrimaryScreenWidth = screens.Primary?.Bounds.Width ?? default,
                PrimaryScreenHeight = screens.Primary?.Bounds.Height ?? default,
                SumScreenWidth = screens.All.Sum(x => x.Bounds.Width),
                SumScreenHeight = screens.All.Sum(x => x.Bounds.Height),
#endif
                IsAuthenticated = isAuthenticated,
            };
            // 匿名统计与通知公告
            await csc.ActiveUser.Record(req);
        }
        catch (Exception e)
        {
            Log.Error("Startup", e, "ActiveUserPost");
        }
    }
}