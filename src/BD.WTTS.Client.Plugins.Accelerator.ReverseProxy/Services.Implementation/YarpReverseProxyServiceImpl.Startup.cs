// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub/Startup.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class YarpReverseProxyServiceImpl
{
    Action<IServiceCollection>? ipcAddSingletonServices;

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

#if WINDOWS && !REMOVE_DNS_INTERCEPT
        if (ProxyMode == ProxyMode.DNSIntercept)
        {
            services.AddPacketIntercept();
        }
#endif

        // !!! 不可将 IPCSubProcessService ipc 添加到 YarpHost 的 DI 容器中，因停止加速时执行释放会导致进程退出
        //services.AddSingleton(_ => ipc);

        ipcAddSingletonServices?.Invoke(services);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void StartupConfigure(IApplicationBuilder app)
    {
        app.UseHttpLocalRequest();

        app.UseHttpProxyPac();
        app.UseRequestLogging();
        app.UseHttpReverseProxy();

        app.DisableRequestLogging();
    }
}