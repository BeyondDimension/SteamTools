using BD.SteamClient.Services;
using BD.WTTS.Client.Resources;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    const string moduleName = "Authenticator";

    public override string Name => moduleName;

    public override IEnumerable<TabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel()
        {
            ResourceKeyOrName = nameof(Strings.LocalAuth),
            PageType = typeof(AuthenticatorHomePage),
            IsResourceGet = true,
            IconKey = "DefenderApp",
        };
    }

    public override IEnumerable<(Action<IServiceCollection>? @delegate, bool isInvalid, string name)>? GetConfiguration(
        bool directoryExists)
    {
        return base.GetConfiguration(directoryExists);
    }

    public override void ConfigureDemandServices(IServiceCollection services, Startup startup)
    {
        if (startup.HasServerApiClient)
        {
            services.AddSingleton<IAccountPlatformAuthenticatorRepository, AccountPlatformAuthenticatorRepository>();
            services.AddSteamAccountService(c => new SocketsHttpHandler() { CookieContainer = c });
        }
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {
    }
}