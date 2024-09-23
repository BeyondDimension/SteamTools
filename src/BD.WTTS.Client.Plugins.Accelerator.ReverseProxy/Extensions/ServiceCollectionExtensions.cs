using IHttpClientFactory_Common = System.Net.Http.Client.IHttpClientFactory;
using IHttpClientFactory_Extensions_Http = System.Net.Http.IHttpClientFactory;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加由 Yarp 实现的反向代理服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddReverseProxyService(this IServiceCollection services)
    {
        services.AddSingleton<YarpReverseProxyServiceImpl>();
        services.AddSingleton<IReverseProxySettings>(s => s.GetRequiredService<YarpReverseProxyServiceImpl>());
        services.AddSingleton<IReverseProxyService>(s => s.GetRequiredService<YarpReverseProxyServiceImpl>());
        return services;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IServiceCollection AddReverseProxyHttpClient(this IServiceCollection services)
    {
        // https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/ServiceCollectionExtensions.cs#L17
        services.TryAddSingleton<IReverseProxyHttpClientFactory, ReverseProxyHttpClientFactory>();
        return services;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddCommonHttpClientFactory(this IServiceCollection services)
    {
        services.AddSingleton<IHttpClientFactory_Common>(
            s => new HttpClientFactoryWrapper(s));
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddCookieHttpClient(this IServiceCollection services)
    {
        services.AddCommonHttpClientFactory();
        services.AddSingleton<CookieHttpClient>();
        services.AddHttpClient(CookieHttpClient.HttpClientName, (s, c) =>
        {
            c.Timeout = GeneralHttpClientFactory.DefaultTimeout;
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpHandlerType()
        {
            UseCookies = true,
            CookieContainer = CookieHttpClient.CookieContainer,
        });
    }

    /// <summary>
    /// 添加 Http 反向代理
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IServiceCollection AddReverseProxyServer(this IServiceCollection services)
    {
        // https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/ServiceCollectionExtensions.cs#L15

        services.AddCookieHttpClient();

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
            //.AddSingleton<TlsInvadeMiddleware>()
            //.AddSingleton<TlsRestoreMiddleware>()

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IServiceCollection AddFlowAnalyze(this IServiceCollection services)
    {
        // https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.FlowAnalyze/ServiceCollectionExtensions.cs#L16
        return services.AddSingleton<IFlowAnalyzer, FlowAnalyzer>();
    }

#if WINDOWS

#if !REMOVE_DNS_INTERCEPT
    /// <summary>
    /// 注册数据包拦截器
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

#endif
}