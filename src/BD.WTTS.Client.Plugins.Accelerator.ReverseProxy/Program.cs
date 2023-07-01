using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;

const string moduleName = "Accelerator";
const string pluginName = "Accelerator";
#if DEBUG
Console.WriteLine($"This: {moduleName} / Program.Start");
#endif
try
{
    var exitCode = await IPCSubProcessService.MainAsync(moduleName, pluginName, ConfigureServices, static ipcProvider =>
    {
        // 添加反向代理服务（供主进程的 IPC 远程访问）
        ipcProvider.CreateIpcJoint(LazyReverseProxyServiceImpl.Instance);
    }, args);

    return exitCode;
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    Console.ReadLine();
    return 500;
}

static void ConfigureServices(IServiceCollection services)
{
    services.AddLogging(l =>
    {
        l.ClearProviders();
        l.AddNLog(LogManager.Configuration); // 添加 NLog 日志
#if ((WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)) && DEBUG
        l.AddConsole();
#endif
    });

    services.AddHttpClient();
    services.AddCommonHttpClientFactory();

    services.AddDnsAnalysisService();
    // 添加反向代理服务（子进程实现）
    services.AddReverseProxyService();
    services.AddSingleton<ICertificateManager, CertificateManagerImpl>();
}