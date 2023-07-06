using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    const string moduleName = "GameList";

    public override string Name => moduleName;

    public override IEnumerable<TabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel()
        {
            ResourceKeyOrName = nameof(Strings.GameList),
            PageType = typeof(GameListPage),
            IsResourceGet = true,
            IconKey = "Game",
        };
    }

    public override void ConfigureDemandServices(IServiceCollection services, Startup startup)
    {
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {
        services.AddSingleton<IPartialGameLibrarySettings>(s =>
            s.GetRequiredService<IOptionsMonitor<GameLibrarySettings_>>().CurrentValue);
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }

    public override IEnumerable<(Action<IServiceCollection>? @delegate, bool isInvalid, string name)>? GetConfiguration(bool directoryExists)
    {
        yield return GetConfiguration<GameLibrarySettings_>(directoryExists);
    }
}
