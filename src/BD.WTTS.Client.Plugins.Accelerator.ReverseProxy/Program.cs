using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;

const string moduleName = "Accelerator";
try
{
    var exitCode = await IPCSubProcessService.MainAsync(moduleName, ConfigureServices, static ipcProvider =>
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
        l.AddConsole();
    });

    services.AddDnsAnalysisService();
    // 添加反向代理服务（子进程实现）
    services.AddReverseProxyService();
    services.AddSingleton<ICertificateManager, CertificateManagerImpl>();
}