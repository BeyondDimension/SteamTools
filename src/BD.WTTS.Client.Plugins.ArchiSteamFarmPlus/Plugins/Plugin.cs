using BD.WTTS.Properties;
using BD.WTTS.UI.Views.Pages;
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
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    const string moduleName = AssemblyInfo.ArchiSteamFarmPlus;

    public override Guid Id => Guid.Parse(AssemblyInfo.ArchiSteamFarmPlusId);

    public sealed override string UniqueEnglishName => moduleName;

    public override string Name => BDStrings.ArchiSteamFarmPlus;

    protected sealed override string? AuthorOriginalString => null;

    public sealed override string Description => moduleName + " 控制台功能实现";

    public sealed override object? Icon => Resources.asf;

    public override IEnumerable<MenuTabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel(this, nameof(BDStrings.ArchiSteamFarmPlus))
        {
            PageType = typeof(MainFramePage),
            IsResourceGet = true,
            //IconKey = "GameConsole",
            IconKey = Icon
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
        //ArchiSteamFarm.Web.WebBrowser.CreateHttpHandlerDelegate = CreateHttpHandler;
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

    public override IEnumerable<(Action<IServiceCollection>? @delegate, bool isInvalid, string name)>? GetConfiguration(bool directoryExists)
    {
        yield return GetConfiguration<ASFSettings_>(directoryExists);
    }

    public override async ValueTask OnExit()
    {
        if (ArchiSteamFarmServiceImpl.ASFProcessId.HasValue)
        {
            try
            {
                var process = Process.GetProcessById(ArchiSteamFarmServiceImpl.ASFProcessId.Value);
                process.Kill();
                process.Dispose();
            }
            catch (ArgumentException)
            {
                // 进程已经退出
            }
        }
        await base.OnExit();
    }
}
