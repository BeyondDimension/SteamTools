using CreateHttpHandlerArgs = System.ValueTuple<
    bool,
    System.Net.DecompressionMethods,
    System.Net.CookieContainer,
    System.Net.IWebProxy?,
    int>;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
sealed class Plugin : PluginBase<Plugin>
{
    const string moduleName = "ArchiSteamFarmPlus";

    public override string Name => moduleName;

    public override IEnumerable<TabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel()
        {
            ResourceKeyOrName = "ArchiSteamFarmPlus",
            PageType = null,
            IsResourceGet = true,
            IconKey = "ASF",
        };
    }

    public override void ConfigureDemandServices(IServiceCollection services, Startup startup)
    {
        if (startup.HasSteam)
        {
            // ASF Service
            services.AddArchiSteamFarmService();
        }
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {
        ArchiSteamFarm.Web.WebBrowser.CreateHttpHandlerDelegate = CreateHttpHandler;
    }

    /// <summary>
    /// 用于 <see cref="ArchiSteamFarm"/> 的 <see cref="HttpMessageHandler"/>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    static HttpMessageHandler? CreateHttpHandler(CreateHttpHandlerArgs args)
    {
        var proxy = args.Item4;
        var useProxy = GeneralHttpClientFactory.UseWebProxy(proxy);
        var setMaxConnectionsPerServer = !(args.Item5 < 1);  // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/SocketsHttpHandler.cs#L157
#if NETCOREAPP2_1_OR_GREATER
        {
            var handler = new SocketsHttpHandler()
            {
                AllowAutoRedirect = args.Item1,
                AutomaticDecompression = args.Item2,
                CookieContainer = args.Item3,
            };
            if (useProxy)
            {
                handler.Proxy = proxy;
                handler.UseProxy = true;
            }
            if (setMaxConnectionsPerServer)
            {
                handler.MaxConnectionsPerServer = args.Item5;
            }
            return handler;
        }
#elif ANDROID
        var handler = PlatformHttpMessageHandlerBuilder.CreateAndroidClientHandler(new()
        {
            AllowAutoRedirect = args.Item1,
            AutomaticDecompression = args.Item2,
            CookieContainer = args.Item3,
        });
        if (useProxy)
        {
            handler.Proxy = proxy;
            handler.UseProxy = true;
        }
        if (setMaxConnectionsPerServer)
        {
            handler.MaxConnectionsPerServer = args.Item5;
        }
        return handler;
#else
        return null!;
#endif
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }
}
