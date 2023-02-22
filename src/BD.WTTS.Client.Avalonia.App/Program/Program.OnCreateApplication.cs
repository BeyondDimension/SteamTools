// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class Program
{
    /// <summary>
    /// 在创建 App 前执行的初始化
    /// </summary>
    /// <param name="isTrace"></param>
    static void OnCreateAppExecuting(bool isTrace = false)
    {
        bool isDesignMode = Design.IsDesignMode;

        if (isTrace) StartWatchTrace.Record();
        try
        {
#if MACOS || MACCATALYST || IOS
            MacCatalystFileSystem.InitFileSystem();
#elif LINUX
            LinuxFileSystem.InitFileSystem();
#elif WINDOWS && !WINDOWS_DESKTOP_BRIDGE
            WindowsFileSystem.InitFileSystem();
#elif ANDROID
            FileSystemEssentials.InitFileSystem();
#else
            FileSystem2.InitFileSystem();
#endif
            if (isTrace) StartWatchTrace.Record("FileSystem");

            IApplication.InitLogDir(isTrace: isTrace);
            if (isTrace) StartWatchTrace.Record("InitLogDir");

            GlobalExceptionHelpers.Init();
            if (isTrace) StartWatchTrace.Record("ExceptionHandler");

            if (!isDesignMode)
            {
#if DBREEZE
                SettingsProviderV3.Migrate();
                if (isTrace) StartWatchTrace.Record("SettingsHost.Migrate");
                PreferencesPlatformServiceImplV2.Migrate();
                if (isTrace) StartWatchTrace.Record("Preferences.Migrate");
#endif
                SettingsHost.Load();
                if (isTrace) StartWatchTrace.Record("SettingsHost");
            }
        }
        finally
        {
            if (isTrace) StartWatchTrace.Record(dispose: true);
        }

        ArchiSteamFarm.Web.WebBrowser.CreateHttpHandlerDelegate = CreateHttpHandler;
    }

    /// <summary>
    /// 在创建 App 时执行的初始化
    /// </summary>
    /// <param name="host"></param>
    /// <param name="handlerViewModelManager"></param>
    /// <param name="isTrace"></param>
    static void OnCreateAppExecuted(IApplication.IProgramHost host, Action<IViewModelManager>? handlerViewModelManager = null, bool isTrace = false)
    {
        if (isTrace) StartWatchTrace.Record();
        try
        {
            host.Application.PlatformInitSettingSubscribe();
            if (isTrace) StartWatchTrace.Record("SettingSubscribe");

            var vmService = IViewModelManager.Instance;
            vmService.InitViewModels();
            if (isTrace) StartWatchTrace.Record("VM.Init");

            handlerViewModelManager?.Invoke(vmService);
            vmService.MainWindow?.Initialize();
            if (isTrace) StartWatchTrace.Record("VM.Delegate");
        }
        finally
        {
            if (isTrace) StartWatchTrace.Record(dispose: true);
        }
    }
}