using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using IHttpClientFactory_Common = System.Net.Http.Client.IHttpClientFactory;
using IHttpClientFactory_Extensions_Http = System.Net.Http.IHttpClientFactory;

const string moduleName = "Accelerator";
#if DEBUG
Console.WriteLine($"This: {moduleName} / Program.Start");
#endif
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

    services.AddHttpClient();
    services.AddSingleton<IHttpClientFactory_Common>(
             s => new HttpClientFactoryWrapper(s));

    services.AddDnsAnalysisService();
    // 添加反向代理服务（子进程实现）
    services.AddReverseProxyService();
    services.AddSingleton<ICertificateManager, CertificateManagerImpl>();
}

sealed class HttpClientFactoryWrapper : IHttpClientFactory_Common
{
    readonly IServiceProvider s;

    public HttpClientFactoryWrapper(IServiceProvider s)
    {
        this.s = s;
    }

    HttpClient IHttpClientFactory_Common.CreateClient(string name, HttpHandlerCategory category)
        => s.GetRequiredService<IHttpClientFactory_Extensions_Http>().CreateClient(name);
}