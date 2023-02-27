#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub/Startup.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class YarpReverseProxyServiceImpl
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void StartupConfigureServices(IServiceCollection services)
    {
        services.Configure<HostOptions>(hostOptions =>
        {
            // Windows 上有一些情况下无法监听某些端口会抛出异常，忽略后台服务中的异常避免整个 Host 中止
            hostOptions.BackgroundServiceExceptionBehavior =
                BackgroundServiceExceptionBehavior.Ignore;
        });

        services.AddConfiguration(this);
        services.AddDomainResolve();
        services.AddReverseProxyHttpClient();
        services.AddReverseProxyServer();
        services.AddFlowAnalyze();

#if WINDOWS
        if (ProxyMode == ProxyMode.DNSIntercept)
        {
            services.AddPacketIntercept();
        }
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void StartupConfigure(IApplicationBuilder app)
    {
        app.UseHttpLocalRequest();

        app.UseHttpProxyPac();
        app.UseRequestLogging();
        app.UseHttpReverseProxy();

        //app.UseRouting();
        app.DisableRequestLogging();

        //app.UseEndpoints(endpoint =>
        //{
        //    endpoint.MapGet("/flowStatistics", context =>
        //    {
        //        var flowStatistics = context.RequestServices.GetRequiredService<IFlowAnalyzer>().GetFlowStatistics();
        //        return context.Response.WriteAsJsonAsync(flowStatistics);
        //    });
        //});
    }
}
#endif