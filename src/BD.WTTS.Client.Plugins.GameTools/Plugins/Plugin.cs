namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
sealed class Plugin : PluginBase<Plugin>
{
    public override string Name => nameof(TabItemViewModel.TabItemId.GameTools);

    public override void ConfigureDemandServices(IServiceCollection services, IApplication.IStartupArgs args, StartupOptions options)
    {
    }

    public override void ConfigureRequiredServices(IServiceCollection services, IApplication.IStartupArgs args, StartupOptions options)
    {
    }

    public override ValueTask OnLoaded()
    {
        return ValueTask.CompletedTask;
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }
}
