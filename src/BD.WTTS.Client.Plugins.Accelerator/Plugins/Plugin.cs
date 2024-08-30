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

    public sealed override string UniqueEnglishName => moduleName;

    public sealed override string Description => "提供一些游戏相关网站服务的加速及脚本注入功能";

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

    readonly TaskCompletionSource<IReverseProxyService> reverseProxyService = new();
    readonly TaskCompletionSource<ICertificateManager> certificateManager = new();
    //readonly TaskCompletionSource<IAcceleratorService> acceleratorService = new();

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
        var ipc = IPCMainProcessService.Instance;

        // 启动加速模块子进程
        await ipc.AddDaemonWithStartSubProcessAsync(moduleName, async ipc =>
        {
            var subProcessPath = SubProcessPath;
            var p = await ipc.StartSubProcessAsync(subProcessPath.ThrowIsNull(),
                isAdministrator: true);
            return p;
        });
    }

    public override async ValueTask OnPeerConnected(bool isReconnected)
    {
        if (!isReconnected)
        {
            var ipc = IPCMainProcessService.Instance;

            // 从子进程中获取 IPC 远程服务
            await GetIpcRemoteServiceAsync(moduleName, ipc, reverseProxyService);
            await GetIpcRemoteServiceAsync(moduleName, ipc, certificateManager);
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

            //if (ResourceService.IsChineseSimplified)
            //{
            await MainThread2.InvokeOnMainThreadAsync(ProxyService.Current.InitializeAsync);
            //}
        }
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {
        cfg.AddProfile<AcceleratorAutoMapperProfile>();
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
                    subProcessPath = Assembly.GetExecutingAssembly().Location;
                    subProcessPath = Path.GetDirectoryName(subProcessPath);
                    subProcessPath.ThrowIsNull();

                    const string fileName = $"Steam++.{moduleName}";
                    var subProcessFileName = OperatingSystem.IsWindows() ? $"{fileName}{FileEx.EXE}" : fileName;
                    subProcessPath = Path.Combine(subProcessPath, subProcessFileName);

#if DEBUG // DEBUG 模式遍历项目查找模块
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
