namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
sealed class Plugin : PluginBase<Plugin>, IGameLibrarySettings
{
    public override string Name => nameof(TabItemViewModel.TabItemId.GameList);

    public override void ConfigureDemandServices(IServiceCollection services, IApplication.IStartupArgs args, StartupOptions options)
    {
    }

    public override void ConfigureRequiredServices(IServiceCollection services, IApplication.IStartupArgs args, StartupOptions options)
    {
        services.AddSingleton<IGameLibrarySettings>(_ => this);
    }

    public override ValueTask OnLoadedAsync()
    {
        return ValueTask.CompletedTask;
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }

    Dictionary<uint, string?> IGameLibrarySettings.HideGameList => GameLibrarySettings.HideGameList.Value!;

    Dictionary<uint, string?>? IGameLibrarySettings.AFKAppList => GameLibrarySettings.AFKAppList.Value;

    bool IGameLibrarySettings.IsAutoAFKApps => GameLibrarySettings.IsAutoAFKApps.Value;
}
