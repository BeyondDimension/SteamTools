using BD.WTTS.Properties;
using BD.WTTS.UI.Views.Pages;
using BD.WTTS.UI.ViewModels;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    const string moduleName = AssemblyInfo.Authenticator;

    public override Guid Id => Guid.Parse(AssemblyInfo.AuthenticatorId);

    public sealed override string Name => Strings.LocalAuth;

    public override string? NameResourceKey => nameof(Strings.LocalAuth);

    public sealed override string UniqueEnglishName => moduleName;

    public sealed override string Description => Strings.L10nAuthIdle_Text029;

    public override string? DescriptionResourceKey => nameof(Strings.L10nAuthIdle_Text029);

    protected sealed override string? AuthorOriginalString => null;

    public sealed override object? Icon => Resources.authenticator; //"avares://BD.WTTS.Client.Plugins.Authenticator/UI/Assets/authenticator.ico";

    public override IEnumerable<MenuTabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel(this, nameof(Strings.LocalAuth))
        {
            //PageType = typeof(MainFramePage),
            PageType = typeof(AuthenticatorHomePage),
            IsResourceGet = true,
            IconKey = Icon,
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
            services.AddSingleton<ISteamSavedAccountProvider, AuthenticatorSavedSteamAccounts>();
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