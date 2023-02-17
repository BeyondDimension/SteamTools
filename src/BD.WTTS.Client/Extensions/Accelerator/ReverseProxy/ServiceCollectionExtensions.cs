#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加由 Yarp 实现的反向代理服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddReverseProxyService(this IServiceCollection services)
    {
        services.AddSingleton<IReverseProxyService, YarpReverseProxyServiceImpl>();
        return services;
    }

    internal static IServiceCollection AddConfiguration(this IServiceCollection services, YarpReverseProxyServiceImpl reverseProxyService)
    {
        // https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/ServiceCollectionExtensions.cs#L18
        TypeConverterBinder.Bind(IPAddress2.ParseNullable, val => val?.ToString());
        TypeConverterBinder.Bind(IPEndPoint.Parse, val => val?.ToString());

        // reverseProxyService 不能直接添加进服务集合，因 host 会释放 service
        services.AddSingleton<IReverseProxyConfig>(new ReverseProxyConfig(reverseProxyService));
        return services;
    }

    /// <summary>
    /// 注册域名解析相关服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    internal static IServiceCollection AddDomainResolve(this IServiceCollection services)
    {
        // https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.DomainResolve/ServiceCollectionExtensions.cs#L17
        //services.TryAddSingleton<DnsClient>();
        //services.TryAddSingleton<DnscryptProxy>();
        //services.TryAddSingleton<PersistenceService>();
        //services.TryAddSingleton<IPAddressService>();
        services.TryAddSingleton<IDomainResolver, DomainResolver>();
        //services.AddHostedService<DomainResolveHostedService>();
        return services;
    }

    /// <summary>
    /// 添加 HttpClient 相关服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    internal static IServiceCollection AddReverseProxyHttpClient(this IServiceCollection services)
    {
        // https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/ServiceCollectionExtensions.cs#L17
        services.TryAddSingleton<IReverseProxyHttpClientFactory, ReverseProxyHttpClientFactory>();
        return services;
    }

    /// <summary>
    /// 添加 Http 反向代理
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    internal static IServiceCollection AddReverseProxyServer(this IServiceCollection services)
    {
        // https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/ServiceCollectionExtensions.cs#L15

        CookieHttpClient.AddHttpClient(services);

        return services
            .AddMemoryCache()
            .AddHttpForwarder()
            .AddSingleton<CertService>()
            //.AddSingleton<ICaCertInstaller, CaCertInstallerOfMacOS>()
            //.AddSingleton<ICaCertInstaller, CaCertInstallerOfWindows>()
            //.AddSingleton<ICaCertInstaller, CaCertInstallerOfLinuxRedHat>()
            //.AddSingleton<ICaCertInstaller, CaCertInstallerOfLinuxDebian>()

            // tcp
            .AddSingleton<HttpProxyMiddleware>()
            .AddSingleton<TunnelMiddleware>()

            // tls
            .AddSingleton<TlsInvadeMiddleware>()
            .AddSingleton<TlsRestoreMiddleware>()

            //http
            .AddSingleton<HttpLocalRequestMiddleware>()
            .AddSingleton<HttpProxyPacMiddleware>()
            .AddSingleton<RequestLoggingMiddleware>()
            .AddSingleton<HttpReverseProxyMiddleware>();
    }

    /// <summary>
    /// 添加流量分析
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    internal static IServiceCollection AddFlowAnalyze(this IServiceCollection services)
    {
        // https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.FlowAnalyze/ServiceCollectionExtensions.cs#L16
        return services.AddSingleton<IFlowAnalyzer, FlowAnalyzer>();
    }

#if WINDOWS

    /// <summary>
    /// 注册数据包拦截器
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    internal static IServiceCollection AddPacketIntercept(this IServiceCollection services)
    {
        // https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/ServiceCollectionExtensions.cs#L21
        //services.AddSingleton<IDnsConflictSolver, HostsConflictSolver>();
        //services.AddSingleton<IDnsConflictSolver, ProxyConflictSolver>();
        services.TryAddSingleton<IDnsInterceptor, DnsInterceptor>();
        services.AddHostedService<DnsInterceptHostedService>();

        services.AddSingleton<ITcpInterceptor, SshInterceptor>();
        services.AddSingleton<ITcpInterceptor, GitInterceptor>();
        services.AddSingleton<ITcpInterceptor, HttpInterceptor>();
        services.AddSingleton<ITcpInterceptor, HttpsInterceptor>();
        services.AddHostedService<TcpInterceptHostedService>();

        return services;
    }

#endif
}
#endif