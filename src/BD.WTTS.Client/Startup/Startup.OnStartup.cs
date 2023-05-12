// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup // OnStartup
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void InitVisualStudioAppCenterSDK()
    {

    }

    public virtual void InitSettingSubscribe()
    {
        var a = IApplication.Instance;

        UISettings.Theme.Subscribe(x => a.Theme = (AppTheme)x);
        UISettings.Language.Subscribe(ResourceService.ChangeLanguage);
    }

    public virtual void OnStartup()
    {
        StartupToastIntercept.OnStartuped();

#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Start();
#endif
        InitVisualStudioAppCenterSDK();
#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Record("VisualStudioAppCenter");
#endif

        if (IsMainProcess)
        {
            Task.Run(async () =>
            {
                await ActiveUserRecordAsync(ActiveUserAnonymousStatisticType.OnStartup);
            });
            if (GeneralSettings.IsAutoCheckUpdate.Value)
            {
                Task.Run(async () =>
                {
                    await IApplicationUpdateService.Instance
                        .CheckUpdateAsync(showIsExistUpdateFalse: false);
                });
            }
        }

#if DEBUG
        DebugConsole.WriteInfo();
#endif

#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Stop();
#endif
    }

    protected abstract ActiveUserRecordDTO GetActiveUserRecord();

    [MethodImpl(MethodImplOptions.NoInlining)]
    async Task ActiveUserRecordAsync(ActiveUserAnonymousStatisticType type)
    {
        if (!IsMainProcess)
            return;

        try
        {
            var userService = UserService.Current;
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

            var request = GetActiveUserRecord();
            request.Type = type;
            request.IsAuthenticated = isAuthenticated;
            await csc.ActiveUser.Record(request);
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Handler(ex, nameof(ActiveUserRecordAsync));
        }
    }
}