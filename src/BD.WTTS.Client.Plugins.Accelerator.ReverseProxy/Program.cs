using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;

const string moduleName = AssemblyInfo.Accelerator;
const string pluginName = AssemblyInfo.Accelerator;
#if DEBUG
//Console.WriteLine($"This: {moduleName} / Program.Start");
var consoleTitle = $"[{Environment.ProcessId}, {IsProcessElevated_DEBUG_Only().ToLowerString()}] {Constants.CUSTOM_URL_SCHEME_NAME}({moduleName}) {string.Join(' ', Environment.GetCommandLineArgs().Skip(1))}";
SetConsoleTitle(consoleTitle);

[MethodImpl(MethodImplOptions.AggressiveInlining)]
static void SetConsoleTitle(string title)
{
    try
    {
        Console.Title = title;
    }
    catch
    {

    }
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
static bool IsProcessElevated_DEBUG_Only()
{
    // use WindowsPlatformServiceImpl.IsProcessElevated on not Debug
    using WindowsIdentity identity = WindowsIdentity.GetCurrent();
    WindowsPrincipal principal = new(identity);
    return principal.IsInRole(WindowsBuiltInRole.Administrator);
}
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
#if DEBUG
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