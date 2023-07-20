using BD.WTTS.Properties;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    const string moduleName = AssemblyInfo.Authenticator;

    public override Guid Id => Guid.Parse(AssemblyInfo.AuthenticatorId);

    public sealed override string Name => Strings.LocalAuth;

    public sealed override string UniqueEnglishName => moduleName;

    public sealed override string Description => "提供多平台多账号令牌管理、加密、确认交易、云同步等功能";

    protected sealed override string? AuthorOriginalString => null;

    public sealed override object? Icon => new MemoryStream(Resources.authenticator); //"avares://BD.WTTS.Client.Plugins.Authenticator/UI/Assets/authenticator.ico";

    public override IEnumerable<TabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel()
        {
            ResourceKeyOrName = nameof(Strings.LocalAuth),
            PageType = typeof(MainFramePage),
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