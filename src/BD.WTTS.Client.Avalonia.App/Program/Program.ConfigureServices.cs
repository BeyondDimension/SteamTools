// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class Program
{
    static bool isInitialized;

    /// <summary>
    /// 初始化启动
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ConfigureServices(IApplication.IStartupArgs args, AppServicesLevel level, IServiceCollection? services = null, bool isTrace = false)
    {
        if (!isInitialized)
        {
            isInitialized = true;
            var options = new StartupOptions(level);
            if (options.HasServerApiClient)
            {
                ModelValidatorProvider.Init();
                if (isTrace) StartWatchTrace.Record("ModelValidatorProvider.Init");
            }
            if (isTrace) StartWatchTrace.Record($"InitDI: {level}");
            if (services == null)
            {
                // new ServiceCollection 创建服务集合
                ConfigureServices(args, options, isTrace);
                OnBuild(isTrace);
            }
            else
            {
                // 使用传入的 ServiceCollection
                // 但需要在完成时调用 DI.ConfigureServices(IServiceProvider) 与 OnBuild
                ConfigureServices(services, args, options);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void OnBuild(bool isTrace = false)
    {
        VersionTracking2.Track();
        if (isTrace) StartWatchTrace.Record($"VersionTracking2.Track");
        Migrations.Up();
        if (isTrace) StartWatchTrace.Record($"Migrations.Up");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ConfigureServices(IApplication.IStartupArgs args, StartupOptions options, bool isTrace = false)
    {
        Ioc.ConfigureServices(s => ConfigureServices(s, args, options, isTrace));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ConfigureServices(IServiceCollection services, IApplication.IStartupArgs args, StartupOptions options, bool isTrace = false)
    {
        ConfigureDemandServices(services, args, options, isTrace);
        if (isTrace) StartWatchTrace.Record("DI.D");
        ConfigureRequiredServices(services, args, options, isTrace);
        if (isTrace) StartWatchTrace.Record("DI.ConfigureRequiredServices");
    }

    sealed class Essentials_AppVerS : IApplicationVersionService
    {
        string IApplicationVersionService.ApplicationVersion => AssemblyInfo.Version;

        string IApplicationVersionService.AssemblyTrademark => AssemblyInfo.Trademark;
    }

    /// <summary>
    /// 配置任何进程都必要的依赖注入服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <param name="isTrace"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ConfigureRequiredServices(IServiceCollection services, IApplication.IStartupArgs args, StartupOptions options, bool isTrace = false)
    {
        services.AddSingleton(_ => Host.Instance.App);
        services.AddSingleton(Host.Instance);

#if WINDOWS && !WINDOWS_DESKTOP_BRIDGE
        services.AddScheduledTaskService();
#endif
        // 平台服务 此项放在其他通用业务实现服务之前
        services.AddPlatformService(options);
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
        // Essentials
        services.TryAddEssentials<Essentials_AppVerS>();

        // 添加安全服务
        services.AddSecurityService<EmbeddedAesDataProtectionProvider, LocalDataProtectionProvider>();
    }

    static IDisposable? PlatformApp
    {
        get
        {
            try
            {
                return Host.Instance.App;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 配置按需使用的依赖注入服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <param name="isTrace"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ConfigureDemandServices(IServiceCollection services, IApplication.IStartupArgs args, StartupOptions options, bool isTrace = false)
    {
        if (isTrace) StartWatchTrace.Record("DI.D.Calc");
        services.AddDnsAnalysisService();
        if (options.HasGUI)
        {
            services.AddPinyin();
#if MAUI || ANDROID || IOS
            services.TryAddFontManager();
#elif AVALONIA
            services.TryAddAvaloniaFontManager(useGdiPlusFirst: true);
#else
#endif
            // 添加 Toast 提示服务
            services.AddStartupToastIntercept();

            services.TryAddToast();

            services.AddSingleton<IApplication>(s => s.GetRequiredService<App>());

            // 添加主线程助手(MainThreadDesktop)
            services.AddMainThreadPlatformService();

            //services.TryAddAvaloniaFilePickerPlatformService();

            #region MessageBox

            /* System.Windows.MessageBox 在 WPF 库中，仅支持 Win 平台
             * 改为 System.Windows.MessageBoxCompat 可跨平台兼容
             * 在其他平台上使用 MessageBox.Avalonia 库实现
             * API变更说明：
             * - 如果需要获取返回值，即点击那个按钮，则使用异步版本 ShowAsync
             * - 如果不需要获取返回值，则可直接使用 同步版本 Show
             * 注意事项：
             * - 图标(Icon)与按钮(Button)不要使用标记为 Obsolete 的
             * - WPF 中 显示窗口(Show)会锁死父窗口等，类似 ShowDialog
             * - MessageBox.Avalonia 中则不会锁死窗口
             * 已知问题：
             * - 在 MessageBox.Avalonia 中
             *  - 如果内容文本(messageBoxText)过短 UI 上的文字显示不全
             *  - 点击窗口按 Ctrl+C 无法复制弹窗中的文本内容
             *  - 按钮文本(ButtonText)缺少本地化翻译(Translate)
             *  - 某些图标图片与枚举值不太匹配，例如 Information
             */
            services.TryAddWindowManager();

            #endregion

            // 添加管理主窗口服务
            services.AddViewModelManager();

            if (isTrace) StartWatchTrace.Record("DI.D.GUI");
        }
        if (options.HasHttpClientFactory || options.HasServerApiClient
            )
        {
            // 添加 Http 平台助手桌面端或移动端实现
            services.TryAddClientHttpPlatformHelperService();
            if (isTrace) StartWatchTrace.Record("DI.D.ClientHttpPlatformHelperService");
        }

        if (options.HasHttpClientFactory)
        {
#if ANDROID || IOS
            // 添加 HttpClientFactory 平台原生实现
            services.AddNativeHttpClient();
#endif
            // 通用 Http 服务
            services.AddSingleton<IHttpClientFactory, LiteHttpClientFactory>();
            if (isTrace) StartWatchTrace.Record("DI.D.HttpClientFactory");
        }
        services.TryAddScriptManager();
        if (isTrace) StartWatchTrace.Record("DI.D.ScriptManager");

        if (options.HasHttpProxy)
        {
            // 通用 Http 代理服务
            services.AddReverseProxyService();
            if (isTrace) StartWatchTrace.Record("DI.D.HttpProxy");
        }
        if (options.HasServerApiClient)
        {
            if (isTrace) StartWatchTrace.Record("DI.D.AppSettings");
            // 添加模型验证框架
            services.TryAddModelValidator();

            // 添加服务端API调用
            services.TryAddMicroServiceClient(configureHandler: ConfigureHandler());

            services.AddAutoMapper();

            // 添加仓储服务
            services.AddRepositories();

            // 业务平台用户管理
            services.TryAddUserManager();
            if (isTrace) StartWatchTrace.Record("DI.D.ServerApiClient");
        }
        // 添加通知服务
        AddNotificationService();
        if (isTrace) StartWatchTrace.Record("DI.D.AddNotificationService");
        void AddNotificationService()
        {
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            if (!args.IsMainProcess) return;
#endif
            services.TryAddNotificationService();
        }
        if (options.HasHosts)
        {
            // hosts 文件助手服务
            services.AddHostsFileService();
            if (isTrace) StartWatchTrace.Record("DI.D.HostsFileService");
        }
        if (options.HasSteam)
        {
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            // Steam 相关助手、工具类服务
            services.AddSteamService();

            // Steamworks LocalApi Service
            services.TryAddSteamworksLocalApiService();
#endif

            // SteamDb WebApi Service
            //services.AddSteamDbWebApiService();

            // Steamworks WebApi Service
            services.AddSteamworksWebApiService();

            // SteamGridDB WebApi Service
            services.AddSteamGridDBWebApiService();

            // ASF Service
            services.AddArchiSteamFarmService();
            if (isTrace) StartWatchTrace.Record("DI.D.Steam");
        }
        if (options.HasMainProcessRequired)
        {
            // 应用程序更新服务
            services.AddApplicationUpdateService();
            if (isTrace) StartWatchTrace.Record("DI.D.AppUpdateService");
        }
    }
}