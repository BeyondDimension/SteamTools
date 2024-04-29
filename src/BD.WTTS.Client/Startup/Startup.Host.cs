// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup // 配置 Host
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void RunUIApplication(
        AppServicesLevel? level = null,
        Func<string>? sendMessage = null,
        params string[] loadModules)
    {
#if DEBUG
        var consoleTitle = Constants.CUSTOM_URL_SCHEME_NAME;
        if (!string.IsNullOrWhiteSpace(ModuleName))
            consoleTitle = $"{consoleTitle}({ModuleName})";
        consoleTitle = $"[{Environment.ProcessId}, {IsProcessElevated_DEBUG_Only().ToLowerString()}] {consoleTitle} {string.Join(' ', Environment.GetCommandLineArgs().Skip(1))}";
        SetConsoleTitle(consoleTitle);

        static bool IsProcessElevated_DEBUG_Only()
        {
#if WINDOWS
            return WindowsPlatformServiceImpl.IsPrivilegedProcess;
#else
            return default;
#endif
        }
#endif

#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Start();
#endif

        level ??= IsMainProcess ? AppServicesLevel.MainProcess : AppServicesLevel.Min;

        HasTrayIcon = level.Value.HasFlag(AppServicesLevel.AppUpdateAndTrayIcon);
        HasUI = level.Value.HasFlag(AppServicesLevel.UI);
        HasServerApiClient = level.Value.HasFlag(AppServicesLevel.ServerApiClient);
        HasRepositories = HasServerApiClient || level.Value.HasFlag(AppServicesLevel.Repositories);
        HasSteam = level.Value.HasFlag(AppServicesLevel.Steam);
        HasHttpClientFactory = HasSteam || level.Value.HasFlag(AppServicesLevel.HttpClientFactory);
        HasHttpProxy = level.Value.HasFlag(AppServicesLevel.HttpProxy);
        HasHosts = level.Value.HasFlag(AppServicesLevel.Hosts);
        HasIPCRoot = level.Value.HasFlag(AppServicesLevel.IPCRoot);
#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Record("AppServicesLevel.HasFlag");
#endif

        var directoryExists = ISettings.DirectoryExists();

        #region 初始化通用【配置/设置】 GeneralSettings

        if (ISettings<GeneralSettings_>.Load(directoryExists, out var @delegate, out var generalSettings))
            InvalidConfigurationFileNames.Add(GeneralSettings_.Name);

        #endregion

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        if (IsMainProcess || HasIPCRoot || loadModules.Length != 0)
        {
            var pluginResults = PluginsCore.InitPlugins(generalSettings?.CurrentValue.DisablePlugins, loadModules);
            var plugins = pluginResults?.Where(x => !x.IsDisable).Select(x => x.Data).ToHashSet();
            HasPlugins = plugins.Any_Nullable();
            if (HasPlugins)
            {
                var pluginIds = new HashSet<Guid>();
                var pluginUniqueEnglishNames = new HashSet<string>();
                Dictionary<string, IPlugin>? pluginRepetitionIds = null;
                Dictionary<string, IPlugin>? pluginRepetitionUniqueEnglishNames = null;
                Dictionary<PluginRepetitionType, Dictionary<string, IPlugin>>? repetitivePlugins = null;
                void AddRepetition(PluginRepetitionType key, Dictionary<string, IPlugin> value)
                {
                    repetitivePlugins ??= new();
                    if (!repetitivePlugins.ContainsKey(key))
                        repetitivePlugins.Add(key, value);
                }
                void AddRepetitionId(IPlugin plugin)
                {
                    pluginRepetitionIds ??= new();
                    AddRepetition(PluginRepetitionType.Id, pluginRepetitionIds);
                    pluginRepetitionIds.Add(plugin.Id.ToString(), plugin);
                }
                void AddRepetitionUniqueEnglishName(IPlugin plugin)
                {
                    pluginRepetitionUniqueEnglishNames ??= new();
                    AddRepetition(PluginRepetitionType.UniqueEnglishName, pluginRepetitionUniqueEnglishNames);
                    pluginRepetitionUniqueEnglishNames.Add(plugin.UniqueEnglishName, plugin);
                }
                foreach (var item in plugins!)
                {
                    var isRepetition = false;
                    if (!pluginIds.Add(item.Id))
                    {
                        isRepetition = true;
                        AddRepetitionId(item);
                    }
                    if (!pluginUniqueEnglishNames.Add(item.UniqueEnglishName))
                    {
                        isRepetition = true;
                        AddRepetitionUniqueEnglishName(item);
                    }
                    if (isRepetition)
                    {
                        pluginResults!.RemoveWhere(x => x.Data == item && !x.IsDisable);
                        plugins.Remove(item);
                    }
                }
                RepetitivePlugins = repetitivePlugins;
                this.plugins = plugins;
            }
            this.pluginResults = pluginResults;
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("InitPlugins");
#endif
        }
#endif

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
            void SetWebProxyMode(AppWebProxyMode mode)
            {
                switch (mode)
                {
                    case AppWebProxyMode.NoProxy:
                        HttpClient.DefaultProxy = HttpNoProxy.Instance;
                        break;
                    case AppWebProxyMode.Custom:
                        WebProxy webProxy;
                        if (!string.IsNullOrEmpty(generalSettings.CurrentValue.CustomWebProxyModeHost) && generalSettings.CurrentValue.CustomWebProxyModePort != default)
                        {
                            webProxy = new(generalSettings.CurrentValue.CustomWebProxyModeHost, generalSettings.CurrentValue.CustomWebProxyModePort)
                            {
                                BypassProxyOnLocal = generalSettings.CurrentValue.CustomWebProxyModeBypassOnLocal,
                            };
                        }
                        else if (!string.IsNullOrEmpty(generalSettings.CurrentValue.CustomWebProxyModeAddress))
                        {
                            webProxy = new(generalSettings.CurrentValue.CustomWebProxyModeAddress, generalSettings.CurrentValue.CustomWebProxyModeBypassOnLocal);
                        }
                        else
                        {
                            SetWebProxyMode(AppWebProxyMode.FollowSystem);
                            return;
                        }
                        if (!string.IsNullOrEmpty(generalSettings.CurrentValue.CustomWebProxyModeCredentialUserName))
                        {
                            NetworkCredential credential = new(generalSettings.CurrentValue.CustomWebProxyModeCredentialUserName,
                                generalSettings.CurrentValue.CustomWebProxyModeCredentialPassword ?? "",
                                generalSettings.CurrentValue.CustomWebProxyModeCredentialDomain ?? "");
                            webProxy.Credentials = credential;
                        }
                        HttpClient.DefaultProxy = webProxy;
                        break;
                    default:
                    case AppWebProxyMode.FollowSystem:
#if WINDOWS
                        // 在 Windows 上还原 .NET Framework 中网络请求跟随系统网络代理变化而动态切换代理行为
                        HttpClient.DefaultProxy = DynamicHttpWindowsProxy.Instance;
#endif
                        break;
                }
            }
            if (generalSettings != null)
                SetWebProxyMode(generalSettings.CurrentValue.WebProxyMode);
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("DynamicHttpWindowsProxy");
#endif
        }

        #region 初始化【配置/设置】 Settings/Configuration

        if (ISettings<UISettings_>.Load(directoryExists, out var @delegate1))
            InvalidConfigurationFileNames.Add(UISettings_.Name);
        else
            @delegate += @delegate1;
        if (ISettings<SteamSettings_>.Load(directoryExists, out var @delegate2))
            InvalidConfigurationFileNames.Add(SteamSettings_.Name);
        else
            @delegate += @delegate2;

        if (TryGetPlugins(out var plugins_cfg))
        {
            foreach (var plugin in plugins_cfg)
            {
                try
                {
                    directoryExists = ISettings.DirectoryExists(plugin.AppDataDirectory);
                    var plugin_cfg = plugin.GetConfiguration(directoryExists);
                    if (plugin_cfg != default)
                    {
                        foreach (var item in plugin_cfg)
                        {
                            if (item.isInvalid)
                            {
                                InvalidConfigurationFileNames.Add(item.name);
                            }
                            if (item.@delegate != null)
                            {
                                @delegate += item.@delegate;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    GlobalExceptionHandler.Handler(ex,
                        $"{plugin.UniqueEnglishName}{nameof(IPlugin.GetConfiguration)}");
                }
            }
        }

        Configuration = @delegate;

#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Record("LoadSettings");
#endif
        #endregion

#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Stop();
#endif

        LoadLogicApplication();

        if (!IsDesignMode && IsMainProcess)
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
                case key_show:
                    try
                    {
                        MainThread2.BeginInvokeOnMainThread(() =>
                        {
                            try
                            {
                                IApplication.Instance.RestoreMainWindow();
                            }
                            catch
                            {

                            }
                        });
                    }
                    catch
                    {

                    }
                    break;
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
                                            var status = value switch
                                            {
                                                OnOffToggle.On => true,
                                                OnOffToggle.Off => false,
                                                _ => !proxyService.ProxyStatus,
                                            };
                                            proxyService.StartOrStopProxyService(status);
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        GlobalExceptionHandler.Handler(ex,
                                            nameof(ProxyMessageReceived));
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

#if STARTUP_WATCH_TRACE || DEBUG
        WatchTrace.Stop();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ConfigureServices(IServiceCollection services)
    {
        #region Configuration

        Configuration?.Invoke(services);

        #endregion

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
                    GlobalExceptionHandler.Handler(ex, $"{plugin.UniqueEnglishName}.ConfigureDemandServices");
                }
                try
                {
                    plugin.ConfigureRequiredServices(services, this);
                }
                catch (Exception ex)
                {
                    GlobalExceptionHandler.Handler(ex, $"{plugin.UniqueEnglishName}.ConfigureRequiredServices");
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