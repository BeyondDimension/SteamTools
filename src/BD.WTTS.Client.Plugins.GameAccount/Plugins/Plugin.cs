namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
sealed class Plugin : PluginBase<Plugin>, ISteamAccountSettings
{
    const string moduleName = "GameAccount";

    public override string Name => moduleName;

    public override IEnumerable<TabItemViewModel>? GetMenuTabItems()
    {
        yield return new GameAccountMenuTabItemViewModel();
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {
        services.AddSingleton<ISteamAccountSettings>(_ => this);
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }

    ConcurrentDictionary<long, string?>? ISteamAccountSettings.AccountRemarks
        => SteamAccountSettings.AccountRemarks;
}
