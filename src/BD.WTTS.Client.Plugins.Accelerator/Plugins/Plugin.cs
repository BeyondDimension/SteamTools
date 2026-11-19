using BD.WTTS.Properties;
using BD.WTTS.UI.Views.Pages;
using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using dotnetCampus.Ipc.Pipes;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    static Plugin()
    {
#if WINDOWS
        XunYouSDK.Initialize();
#endif
    }

    const string moduleName = AssemblyInfo.Accelerator;

    public override Guid Id => Guid.Parse(AssemblyInfo.AcceleratorId);

    public sealed override string Name => Strings.CommunityFix;

    public override string? NameResourceKey => nameof(Strings.CommunityFix);

    public sealed override string UniqueEnglishName => moduleName;

    public sealed override string Description => Strings.L10nAccel_076;

    public override string? DescriptionResourceKey => nameof(Strings.L10nAccel_076);

    protected sealed override string? AuthorOriginalString => null;

    public sealed override object? Icon => Resources.accelerator; //"avares://BD.WTTS.Client.Plugins.Accelerator/UI/Assets/accelerator.ico";

    public override IEnumerable<MenuTabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel(this, nameof(Strings.CommunityFix))
        {
            PageType = typeof(MainFramePage),
            IsResourceGet = true,
            //IconKey = "SpeedHigh",
            IconKey = Icon,
        };

        //yield return new MenuTabItemViewModel()
        //{
        //    ResourceKeyOrName = nameof(Strings.ScriptConfig),
        //    PageType = typeof(ScriptPage),
        //    IsResourceGet = true,
        //    IconKey = "DuplexPortraitOneSided",
        //};
    }

    readonly TaskCompletionSource<IReverseProxyService> reverseProxyService = new(TaskCreationOptions.RunContinuationsAsynchronously);
    readonly TaskCompletionSource<ICertificateManager> certificateManager = new(TaskCreationOptions.RunContinuationsAsynchronously);
    readonly BehaviorSubject<TrayToggleState> trayPreparation = new(TrayToggleState.Loading);
    int proxyInitializationState;
    internal Task ServicesReady { get; }
    //readonly TaskCompletionSource<IAcceleratorService> acceleratorService = new();

    public Plugin()
    {
        ServicesReady = Task.WhenAll(reverseProxyService.Task, certificateManager.Task);
    }

    public override void ConfigureDemandServices(IServiceCollection services, Startup startup)
    {
        services.TryAddScriptManager();

        if (startup.HasHttpProxy)
        {
#if !DISABLE_ASPNET_CORE && (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            // 添加反向代理服务（主进程插件）
            services.AddSingleton(_ => reverseProxyService.Task.GetAwaiter().GetResult());
            services.AddSingleton(_ => certificateManager.Task.GetAwaiter().GetResult());
#endif
        }

        services.AddSingleton<INetworkTestService, NetworkTestService>();

        //if (startup.HasIPCRoot)
        //{
        //    services.AddSingleton<IAcceleratorService, BackendAcceleratorServiceImpl>();
        //}
        //else if (startup.IsMainProcess)
        {
            //services.AddSingleton(_ => acceleratorService.Task.GetAwaiter().GetResult());
            services.AddSingleton<IAcceleratorService, BackendAcceleratorServiceImpl>();
            services.AddSingleton<IXunYouAccelStateToFrontendCallback, XunYouAccelStateToFrontendCallbackImpl>();
        }

        if (startup.HasServerApiClient)
        {
            // 添加仓储服务
            services.AddSingleton<IScriptRepository, ScriptRepository>();
        }
    }

    sealed class XunYouAccelStateToFrontendCallbackImpl : IXunYouAccelStateToFrontendCallback
    {
        public void XunYouAccelStateToFrontendCallback(XunYouAccelStateModel m)
        {
            GameAcceleratorService.Current.XYAccelState = m;
        }
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        services.AddSingleton<IProxyService>(_ => ProxyService.Current);
#endif
    }

    public override void ConfigureServices(IpcProvider ipcProvider, Startup startup)
    {
        //if (startup.HasIPCRoot)
        //{
        //    ipcProvider.CreateIpcJoint(Ioc.Get<IAcceleratorService>());
        //}
        //else if (startup.IsMainProcess)
        //{
        //    ipcProvider.CreateIpcJoint(Ioc.Get<IXunYouAccelStateToFrontendCallback>());
        //}
    }

    public override async ValueTask OnInitializeAsync()
    {
        // Publish availability before the helper connects; it can be delayed or fail to start.
        trayPreparation.OnNext(TrayToggleState.Loading);
        IApplication.Instance.UpdateMenuItems(moduleName, new TrayMenuItem
        {
            Name = Strings.CommunityFix,
            NameResourceKey = nameof(Strings.CommunityFix),
            ToggleState = trayPreparation,
        });

        try
        {
            var ipc = IPCMainProcessService.Instance;
            var process = await ipc.AddDaemonWithStartSubProcessAsync(moduleName, async ipc =>
                await ipc.StartSubProcessAsync(SubProcessPath.ThrowIsNull(), isAdministrator: true));
            if (process == null)
                trayPreparation.OnNext(TrayToggleState.Unavailable);
            else
                _ = ObserveTrayConnectionAsync();
        }
        catch
        {
            trayPreparation.OnNext(TrayToggleState.Unavailable);
            throw;
        }
    }

    async Task ObserveTrayConnectionAsync()
    {
        try
        {
            await ServicesReady.WaitAsync(TimeSpan.FromSeconds(15));
        }
        catch (TimeoutException ex)
        {
            trayPreparation.OnNext(TrayToggleState.Unavailable);
            Log.Warn(nameof(Plugin), ex, "Accelerator helper did not connect in time.");
        }
    }

    public override async ValueTask OnPeerConnected(bool isReconnected)
    {
        if (!reverseProxyService.Task.IsCompletedSuccessfully || !certificateManager.Task.IsCompletedSuccessfully)
        {
            trayPreparation.OnNext(TrayToggleState.Loading);
            var ipc = IPCMainProcessService.Instance;

            // 从子进程中获取 IPC 远程服务
            try
            {
                if (!reverseProxyService.Task.IsCompletedSuccessfully)
                    reverseProxyService.TrySetResult((await ipc.GetServiceAsync<IReverseProxyService>(moduleName)).ThrowIsNull());
                if (!certificateManager.Task.IsCompletedSuccessfully)
                    certificateManager.TrySetResult((await ipc.GetServiceAsync<ICertificateManager>(moduleName)).ThrowIsNull());
            }
            catch (Exception ex)
            {
                // Keep the readiness tasks pending so a later connection can recover.
                trayPreparation.OnNext(TrayToggleState.Unavailable);
                Log.Error(nameof(Plugin), ex, "Accelerator IPC services could not be connected.");
                return;
            }
            //await GetIpcRemoteServiceAsync(IPlatformService.IPCRoot.moduleName, ipc, acceleratorService);
#if DEBUG
            //try
            //{
            //    var debugStringIPC = $"Pid: {Environment.ProcessId}, Exe: {Environment.ProcessPath}, Asm: {Assembly.GetAssembly(GetType())?.FullName}{Environment.NewLine}{reverseProxyService.Task.GetAwaiter().GetResult().GetDebugString()}";
            //    Console.WriteLine($"DebugString/IReverseProxyService: {debugStringIPC}");
            //    var debugStringIPC2 = reverseProxyService.Task.GetAwaiter().GetResult().GetDebugString2().GetAwaiter().GetResult();
            //    Console.WriteLine($"DebugString/IReverseProxyService: {debugStringIPC2}");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
#endif
        }

        if (Interlocked.CompareExchange(ref proxyInitializationState, 1, 0) != 0)
            return;

        try
        {
            await MainThread2.InvokeOnMainThreadAsync(static () => ProxyService.Current.InitializeAsync());
            if (ProxyService.Current.IsInitialized)
                Volatile.Write(ref proxyInitializationState, 2);
        }
        finally
        {
            // A failed attempt must not prevent initialization on the next connection.
            Interlocked.CompareExchange(ref proxyInitializationState, 0, 1);
        }
    }

    public override async ValueTask OnCommandRun(params string[] commandParams)
    {
        if (commandParams is not ["start"])
            throw new ArgumentException("Unknown accelerator command.", nameof(commandParams));
        await ProxyService.Current.StartOrStopProxyService(true);
    }

    public override async ValueTask OnExit()
    {
        try
        {
            await ProxyService.Current.ExitAsync();
            GameAcceleratorSettings.MyGames.Save();
        }
        catch
        {

        }
    }

    string? subProcessPath;

    /// <summary>
    /// 获取子进程文件所在路径
    /// </summary>
    internal string? SubProcessPath
    {
        get
        {
            if (subProcessPath == null)
            {
                try
                {
#if WTTS_NATIVE_AOT
                    subProcessPath = Path.Combine(AppContext.BaseDirectory, "modules", moduleName);
#else
                    subProcessPath = Assembly.GetExecutingAssembly().Location;
                    subProcessPath = Path.GetDirectoryName(subProcessPath);
                    subProcessPath.ThrowIsNull();
#endif

                    const string fileName = $"Steam++.{moduleName}";
                    var subProcessFileName = OperatingSystem.IsWindows() ? $"{fileName}{FileEx.EXE}" : fileName;
                    subProcessPath = Path.Combine(subProcessPath, subProcessFileName);

#if DEBUG && !WTTS_NATIVE_AOT // DEBUG 模式遍历项目查找模块
                    if (!File.Exists(subProcessPath))
                    {
                        subProcessPath = Path.Combine(ProjectUtils.ProjPath, "src", "BD.WTTS.Client.Plugins.Accelerator.ReverseProxy", "bin", "Debug", $"net{Environment.Version.Major}.{Environment.Version.Minor}", subProcessFileName);
                    }
#endif
                    return subProcessPath;
                }
                catch
                {
                    subProcessPath = string.Empty;
                }
            }

            return subProcessPath;
        }
    }

    /// <summary>
    /// 子进程是否存在
    /// </summary>
    /// <returns></returns>
    bool SubProcessExists()
    {
        var subProcessPath = SubProcessPath;
        return !string.IsNullOrWhiteSpace(subProcessPath) && File.Exists(subProcessPath);
    }

    public override bool HasValue([NotNullWhen(false)] out string? error)
    {
        if (!SubProcessExists())
        {
            error = Strings.CommunityFix_SubProcessFileNotExist;
            return false;
        }

        error = default;
        return true;
    }

    public override IEnumerable<(Action<IServiceCollection>? @delegate, bool isInvalid, string name)>? GetConfiguration(bool directoryExists)
    {
        yield return GetConfiguration<ProxySettings_>(directoryExists);
        yield return GetConfiguration<GameAcceleratorSettings_>(directoryExists);
    }
}
