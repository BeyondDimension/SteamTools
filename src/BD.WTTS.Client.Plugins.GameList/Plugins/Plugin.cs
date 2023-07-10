using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    const string moduleName = "GameList";

    public override string Name => Strings.GameList;

    public sealed override string Description => "管理库存游戏";

    public sealed override string Author => "Steam++ 官方";

    public sealed override string? Icon => "avares://BD.WTTS.Client.Plugins.GameList/UI/Assets/game.ico";

    public override IEnumerable<TabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel()
        {
            ResourceKeyOrName = nameof(Strings.GameList),
            PageType = typeof(GameListPage),
            IsResourceGet = true,
            IconKey = Icon,
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
