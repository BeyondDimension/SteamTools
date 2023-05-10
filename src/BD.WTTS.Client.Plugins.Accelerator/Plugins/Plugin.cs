namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
sealed class Plugin : PluginBase<Plugin>
{
    const string moduleName = "Accelerator";

    public sealed override string Name => moduleName;

    IReverseProxyService? reverseProxyService;

    public override void ConfigureDemandServices(IServiceCollection services, Startup startup)
    {
        services.TryAddScriptManager();

        if (startup.HasHttpProxy)
        {
#if !DISABLE_ASPNET_CORE && (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            // 添加反向代理服务（主进程插件）
            services.AddSingleton(_ => reverseProxyService!);
#endif
        }

        if (startup.HasServerApiClient)
        {
            // 添加仓储服务
            services.AddSingleton<IScriptRepository, ScriptRepository>();
        }
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        services.AddSingleton<IProxyService>(_ => ProxyService.Current);
#endif
    }

    public override async ValueTask OnInitializeAsync()
    {
        var ipc = IPCMainProcessService.Instance;

        // 启动加速模块子进程
        ipc.StartProcess(SubProcessPath.ThrowIsNull());

        // 从子进程中获取 IPC 远程服务
        reverseProxyService = await ipc.GetServiceAsync<IReverseProxyService>(moduleName);

        if (ResourceService.IsChineseSimplified)
        {
            await ProxyService.Current.InitializeAsync();
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
            var reverseProxyService = Ioc.Get_Nullable<IReverseProxyService>();
            if (reverseProxyService != null)
            {
                await reverseProxyService.StopProxyAsync();
            }
            ProxyService.OnExitRestoreHosts();
        }
        catch (ObjectDisposedException)
        {

        }
    }

    string? subProcessPath;

    /// <summary>
    /// 获取子进程文件所在路径
    /// </summary>
    string? SubProcessPath
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
                        subProcessPath = Path.Combine(ProjectUtils.ProjPath, "src", "BD.WTTS.Client.Plugins.Accelerator.ReverseProxy", "bin", "Debug", ProjectUtils.tfm, subProcessFileName);
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

    public override bool ExplicitHasValue()
    {
        return SubProcessExists();
    }
}
