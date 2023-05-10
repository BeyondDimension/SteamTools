// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup // 配置 Host
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void RunUIApplication(AppServicesLevel? level = null, Func<string>? sendMessage = null)
    {
#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Start();
#endif

        level ??= IsMainProcess ? AppServicesLevel.MainProcess : AppServicesLevel.Min;

        HasTrayIcon = level.Value.HasFlag(AppServicesLevel.AppUpdateAndTrayIcon);
        HasUI = level.Value.HasFlag(AppServicesLevel.UI);
        HasServerApiClient = level.Value.HasFlag(AppServicesLevel.ServerApiClient);
        HasSteam = level.Value.HasFlag(AppServicesLevel.Steam);
        HasHttpClientFactory = HasSteam || level.Value.HasFlag(AppServicesLevel.HttpClientFactory);
        HasHttpProxy = level.Value.HasFlag(AppServicesLevel.HttpProxy);
        HasHosts = level.Value.HasFlag(AppServicesLevel.Hosts);
#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Record("AppServicesLevel.HasFlag");
#endif
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        if (IsMainProcess)
        {
            plugins = PluginsCore.InitPlugins();
            HasPlugins = plugins.Any_Nullable();
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("InitPlugins");
#endif
        }
#endif
        instance = this;

        if (IsMainProcess)
        {
            if (!InitSingleInstancePipeline(sendMessage))
            {
                return;
            }
            singleInstancePipeline.ThrowIsNull().MessageReceived += OnMessageReceived;
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("InitSingleInstancePipeline");
#endif
        }

        if (HasHttpClientFactory || HasHttpProxy)
        {
#if WINDOWS
            // 在 Windows 上还原 .NET Framework 中网络请求跟随系统网络代理变化而动态切换代理行为
            HttpClient.DefaultProxy = DynamicHttpWindowsProxy.Instance;
#endif
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("DynamicHttpWindowsProxy");
#endif
        }

#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Stop();
#endif
        LoadLogicApplication();

        if (!IsDesignMode)
            StartUIApplication();
    }

    async void OnMessageReceived(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            switch (value)
            {
                case key_shutdown:
                    var app = await UIApplicationTCS.Task;
                    app.Shutdown();
                    return;
                default:
                    var args = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (args.Length >= 1)
                    {
                        switch (args[0])
                        {
                            case key_proxy:
                                if (args.Length >= 2)
                                {
                                    ProxyMessageReceived();
                                    return;
                                }
                                void ProxyMessageReceived()
                                {
                                    var value = Enum.TryParse<OnOffToggle>(
                                        args[1], out var value_) ? value_ : default;
                                    try
                                    {
                                        MainThread2.BeginInvokeOnMainThread(() =>
                                        {
                                            var proxyService = Ioc.Get_Nullable<IProxyService>();
                                            if (proxyService == null)
                                                return;
                                            proxyService.ProxyStatus = value switch
                                            {
                                                OnOffToggle.On => true,
                                                OnOffToggle.Off => false,
                                                _ => !proxyService.ProxyStatus,
                                            };
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalExceptionHandler.Handler(ex, nameof(ProxyMessageReceived));
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        try
        {
            var app = await UIApplicationTCS.Task;
            MainThread2.BeginInvokeOnMainThread(app.RestoreMainWindow);
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Handler(ex, nameof(OnMessageReceived));
        }
    }

    LogLevel? overrideLoggerMinLevel;

    /// <summary>
    /// 加载应用程序逻辑
    /// </summary>
    /// <returns></returns>
    public void LoadLogicApplication()
    {
#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Start();
#endif

        if (HasServerApiClient)
        {
            ModelValidatorProvider.Init();
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("ModelValidatorProvider.Init");
#endif
        }

        // 配置依赖注入服务
        Ioc.ConfigureServices(ConfigureServices);

        if (overrideLoggerMinLevel.HasValue)
        {
            IApplication.LoggerMinLevel = overrideLoggerMinLevel.Value;
        }

        waitConfiguredServices.TrySetResult();

        if (IsMainProcess)
        {
            VersionTracking2.Track();
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("VersionTracking2.Track");
#endif

            Migrations.Up();
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("Migrations.Up");
#endif

            // 仅在主进程中启动 IPC 服务端
            IPCMainProcessService.Instance.Run();
        }

#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Stop();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ConfigureServices(IServiceCollection services)
    {
        ConfigureDemandServices(services);
#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Record("DI.ConfigureDemandServices");
#endif
        ConfigureRequiredServices(services);
#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Record("DI.ConfigureRequiredServices");
#endif

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        if (TryGetPlugins(out var plugins))
        {
            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.ConfigureDemandServices(services, this);
                }
                catch (Exception ex)
                {
                    GlobalExceptionHandler.Handler(ex, $"{plugin.Name}.ConfigureDemandServices");
                }
                try
                {
                    plugin.ConfigureRequiredServices(services, this);
                }
                catch (Exception ex)
                {
                    GlobalExceptionHandler.Handler(ex, $"{plugin.Name}.ConfigureRequiredServices");
                }
            }
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("DI.Plugins.ConfigureServices");
#endif
        }
#endif
    }

    /// <summary>
    /// 配置按需使用的依赖注入服务
    /// </summary>
    /// <param name="services"></param>
    protected abstract void ConfigureDemandServices(IServiceCollection services);

    /// <summary>
    /// 配置任何进程都必要的依赖注入服务
    /// </summary>
    /// <param name="services"></param>
    protected abstract void ConfigureRequiredServices(IServiceCollection services);

    /// <summary>
    /// 启动应用程序
    /// </summary>
    protected abstract void StartUIApplication();

    protected readonly TaskCompletionSource<IApplication> UIApplicationTCS = new();
}