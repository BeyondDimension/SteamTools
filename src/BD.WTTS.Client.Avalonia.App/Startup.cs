// ReSharper disable once CheckNamespace
namespace BD.WTTS;

sealed partial class Program : Startup
{
    static Program? instance;

    public Program(string[]? args = null) : base(args)
    {
#if DEBUG
        Console.WriteLine("args: ");
        if (args != null)
            foreach (var item in args)
            {
                Console.WriteLine(item);
            }
#endif
    }

    protected override void ConfigureRequiredServices(IServiceCollection services)
    {
        services.AddSingleton(_ => UI.App.Instance);
        services.AddSingleton<Startup>(this);

#if WINDOWS
        services.AddScheduledTaskService();
#endif
        // 平台服务 此项放在其他通用业务实现服务之前
        services.AddPlatformService(this);
#if WINDOWS
        //#if !MAUI TODO
        //        services.AddMSAppCenterApplicationSettings();
        //#endif
        services.AddJumpListService();
#endif

        // 添加日志实现
        services.AddGeneralLogging();
#if ANDROID || IOS
        // 添加运行时权限
        services.AddPlatformPermissions();
#endif
        // 添加 app 配置项
        services.TryAddSingleton(Options.Create(AppSettings.Instance));
#if MAUI || ANDROID || IOS
        // 键值对存储 - 由 Essentials 提供
        services.TryAddEssentialsSecureStorage();
#else
        // 键值对存储 - 由 Repository 提供
        services.TryAddRepositorySecureStorage();
        // 首选项(Preferences) - 由 Repository 提供
        services.AddRepositoryPreferences();
#endif

        // 添加主线程助手(MainThreadDesktop)
        services.AddMainThreadPlatformService();

        // Essentials
#if LINUX
        services.TryAddSingleton<IApplicationVersionService, Essentials_AppVerS>();
        services.AddSingleton<IDeviceInfoPlatformService, LinuxDeviceInfoPlatformServiceImpl>();
#else
#if WINDOWS
#if AVALONIA
        services.AddSingleton<IDeviceInfoPlatformService, AvaWinDeviceInfoPlatformServiceImpl>();
#endif
#endif
        services.TryAddEssentials<Essentials_AppVerS>();
#endif

        // 添加安全服务
        services.AddSecurityService<EmbeddedAesDataProtectionProvider, LocalDataProtectionProvider>();

        if (IsMainProcess)
        {
            services.AddSingleton<IPCMainProcessService, IPCMainProcessServiceImpl>();
        }
        else if (HasIPCRoot)
        {
            services.AddSingleton(_ => IPCSubProcessService.Instance);
        }
    }

    protected override void ConfigureDemandServices(IServiceCollection services)
    {
#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Start();
#endif

        if (HasUI)
        {
            services.AddPinyin();
#if MAUI || ANDROID || IOS
            services.TryAddFontManager();
#elif AVALONIA
            //services.TryAddAvaloniaTheme();
            services.TryAddAvaloniaFontManager(useGdiPlusFirst: true);
#else
#endif
            // 添加 Toast 提示服务
            services.AddStartupToastIntercept();

            services.TryAddToast();

            services.AddSingleton<IApplication>(s => s.GetRequiredService<App>());

            services.TryAddAvaloniaFilePickerPlatformService();
#if LINUX
            services.AddSingleton<IClipboardPlatformService, AvaloniaClipboardPlatformService>();
#endif

            #region WindowManager

            services.TryAddWindowManager();
            services.TryAddNavigationService();
            services.TryToastService();

            #endregion

            // 添加视图模型管理服务
            services.AddViewModelManager();

#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("ConfigureDemandServices.UI");
#endif
        }
        if (HasHttpClientFactory || HasServerApiClient)
        {
            // 添加 Http 平台助手桌面端或移动端实现
            services.TryAddClientHttpPlatformHelperService();
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("ConfigureDemandServices.HttpPlatformHelper");
#endif
        }

        if (HasHttpClientFactory)
        {
#if ANDROID || IOS
            // 添加 HttpClientFactory 平台原生实现
            services.AddNativeHttpClient();
#endif
            // 通用 Http 服务
            Fusillade.NetCache.RequestCache = this;
            services.AddFusilladeHttpClient();
            services.TryImageHttpClientService();
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("ConfigureDemandServices.HttpClientFactory");
#endif
        }
        if (HasServerApiClient)
        {
            // 添加模型验证框架
            services.TryAddModelValidator();

            // 添加服务端 API 调用(BD.WTTS.MicroServices.ClientSDK)
            services.TryAddMicroServiceClient();

            services.AddAutoMapper(cfg =>
            {
                if (TryGetPlugins(out var plugins))
                {
                    foreach (var plugin in plugins)
                    {
                        try
                        {
                            plugin.OnAddAutoMapper(cfg);
                        }
                        catch (Exception ex)
                        {
                            GlobalExceptionHandler.Handler(ex,
                                $"{plugin.UniqueEnglishName}{nameof(plugin.OnAddAutoMapper)}");
                        }
                    }
                }
            });

            // 业务平台用户管理
            services.TryAddUserManager();
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("ConfigureDemandServices.ServerApiClient");
#endif
        }

        if (HasRepositories)
        {
            // 添加仓储服务
            services.AddRepositories();
        }

        // 添加通知服务
        AddNotificationService();
        void AddNotificationService()
        {
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            if (!IsMainProcess) return;
#endif
            services.TryAddNotificationService();
        }
#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Record("ConfigureDemandServices.Notification");
#endif

        if (HasHosts || HasIPCRoot)
        {
            // hosts 文件助手服务
            services.AddHostsFileService();
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("ConfigureDemandServices.Hosts");
#endif
        }
        if (HasSteam)
        {
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            // Steam 相关助手、工具类服务
            services.AddSteamService2();

            // Steamworks LocalApi Service
            services.TryAddSteamworksLocalApiService();
#endif

            // SteamDb WebApi Service
            services.AddSteamDbWebApiService();

            // Steamworks WebApi Service
            services.AddSteamworksWebApiService();

            // SteamGridDB WebApi Service
            services.AddSteamGridDBWebApiService();

#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("ConfigureDemandServices.Steam");
#endif
        }
        if (IsMainProcess)
        {
            // 应用程序更新服务
            services.AddApplicationUpdateService();

#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("ConfigureDemandServices.AppUpdate");
#endif
        }

#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Stop();
#endif
    }

    sealed class Essentials_AppVerS : IApplicationVersionService
    {
        string IApplicationVersionService.ApplicationVersion => AssemblyInfo.FileVersion;

        string IApplicationVersionService.AssemblyTrademark => AssemblyInfo.Trademark;
    }

#if LINUX
    sealed class LinuxDeviceInfoPlatformServiceImpl : IDeviceInfoPlatformService
    {
        public string Model => "";

        public string Manufacturer => "";

        public string Name => "";

        public string VersionString => Environment.OSVersion.VersionString;

        public DeviceType DeviceType => DeviceType.Physical;

        public bool IsChromeOS => false;

        public bool IsWinUI => false;

        public bool IsUWP => false;

        public DeviceIdiom Idiom => DeviceIdiom.Desktop;
    }

#endif

#if AVALONIA && WINDOWS
    sealed class AvaWinDeviceInfoPlatformServiceImpl :
        Common.Services.Implementation.Essentials.DeviceInfoPlatformServiceImpl
    {
        public sealed override bool IsWinUI => default;

        public sealed override bool IsUWP => default;
    }
#endif

    public override void InitSettingSubscribe()
    {
        base.InitSettingSubscribe();
        UI.App.Instance.InitSettingSubscribe();
    }

    protected override ActiveUserRecordDTO GetActiveUserRecord()
    {
#if !__MOBILE__ && !MAUI
        var app = UI.App.Instance;
        var window = app.GetFirstOrDefaultWindow();
        var screens = window?.Screens;
#else
        var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
        var mainDisplayInfoH = mainDisplayInfo.Height.ToInt32(NumberToInt32Format.Ceiling);
        var mainDisplayInfoW = mainDisplayInfo.Width.ToInt32(NumberToInt32Format.Ceiling);
#endif
        var result = new ActiveUserRecordDTO
        {
#if __MOBILE__ || MAUI
            ScreenCount = 1,
            PrimaryScreenPixelDensity = mainDisplayInfo.Density,
            PrimaryScreenWidth = mainDisplayInfoW,
            PrimaryScreenHeight = mainDisplayInfoH,
            SumScreenWidth = mainDisplayInfoW,
            SumScreenHeight = mainDisplayInfoH,
#else
            ScreenCount = screens?.ScreenCount ?? default,
            PrimaryScreenPixelDensity = screens?.Primary?.Scaling ?? default,
            PrimaryScreenWidth = screens?.Primary?.Bounds.Width ?? default,
            PrimaryScreenHeight = screens?.Primary?.Bounds.Height ?? default,
            SumScreenWidth = screens?.All.Sum(x => x.Bounds.Width) ?? default,
            SumScreenHeight = screens?.All.Sum(x => x.Bounds.Height) ?? default,
#endif
        };
        return result;
    }
}