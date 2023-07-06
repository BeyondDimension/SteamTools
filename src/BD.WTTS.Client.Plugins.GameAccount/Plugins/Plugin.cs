using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    const string moduleName = "GameAccount";

    public override string Name => moduleName;

    public override IEnumerable<TabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel()
        {
            ResourceKeyOrName = nameof(Strings.UserFastChange),
            PageType = typeof(GameAccountPage),
            IsResourceGet = true,
            IconKey = "SwitchUser",
        };
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {
        services.AddSingleton<IPartialGameAccountSettings>(s =>
            s.GetRequiredService<IOptionsMonitor<GameAccountSettings_>>().CurrentValue);

        services.AddSingleton<IPlatformSwitcher, BasicPlatformSwitcher>()
                .AddSingleton<IPlatformSwitcher, SteamPlatformSwitcher>();
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }

    public override IEnumerable<(Action<IServiceCollection>? @delegate, bool isInvalid, string name)>? GetConfiguration(bool directoryExists)
    {
        yield return GetConfiguration<GameAccountSettings_>(directoryExists);
    }
}
