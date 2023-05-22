using BD.WTTS.UI.Views;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
sealed class Plugin : PluginBase<Plugin>
{
    const string moduleName = "GameAccount";

    public override string Name => moduleName;

    public override IEnumerable<TabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel()
        {
            ResourceKeyOrName = "UserFastChange",
            PageType = typeof(GameAccountPage),
            IsResourceGet = true,
            IconKey = "SwitchUser",
        };
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {

    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }
}
