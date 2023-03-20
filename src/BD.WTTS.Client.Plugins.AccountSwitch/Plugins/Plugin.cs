namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
sealed class Plugin : PluginBase<Plugin>, ISteamAccountSettings
{
    public override string Name => nameof(TabItemViewModel.TabItemId.AccountSwitch);

    public override void ConfigureDemandServices(IServiceCollection services, IApplication.IStartupArgs args, StartupOptions options)
    {
    }

    public override void ConfigureRequiredServices(IServiceCollection services, IApplication.IStartupArgs args, StartupOptions options)
    {
        services.AddSingleton<ISteamAccountSettings>(_ => this);
    }

    public override ValueTask OnLoadedAsync()
    {
        return ValueTask.CompletedTask;
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }

    ConcurrentDictionary<long, string?>? ISteamAccountSettings.AccountRemarks
        => SteamAccountSettings.AccountRemarks;
}
