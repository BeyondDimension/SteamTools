const string moduleName = "Accelerator";
var exitCode = await IPCService.MainAsync(moduleName, ConfigureServices, args);
return exitCode;

static void ConfigureServices(IServiceCollection services)
{
    services.AddLogging(l =>
    {
        l.AddConsole();
    });

    services.AddDnsAnalysisService();
    services.AddReverseProxyService();
    services.AddSingleton<ICertificateManager, CertificateManagerImpl>();
}