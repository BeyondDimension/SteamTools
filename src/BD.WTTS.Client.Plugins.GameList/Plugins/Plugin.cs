namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
sealed class Plugin : PluginBase<Plugin>, IGameLibrarySettings
{
    const string moduleName = "GameList";

    public override string Name => moduleName;

    public override IEnumerable<TabItemViewModel>? GetMenuTabItems()
    {
        yield return new GameListMenuTabItemViewModel();
    }

    public override void ConfigureDemandServices(IServiceCollection services, Startup startup)
    {
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {
        services.AddSingleton<IGameLibrarySettings>(_ => this);
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }

    Dictionary<uint, string?> IGameLibrarySettings.HideGameList => GameLibrarySettings.HideGameList.Value!;

    Dictionary<uint, string?>? IGameLibrarySettings.AFKAppList => GameLibrarySettings.AFKAppList.Value;

    bool IGameLibrarySettings.IsAutoAFKApps => GameLibrarySettings.IsAutoAFKApps.Value;
}
