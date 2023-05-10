namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
sealed class Plugin : PluginBase<Plugin>
{
    public override string Name => nameof(TabItemViewModel.TabItemId.ArchiSteamFarmPlus);

    public override void ConfigureDemandServices(IServiceCollection services, Startup startup)
    {
        if (startup.HasSteam)
        {
            // ASF Service
            services.AddArchiSteamFarmService();
        }
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {
        ArchiSteamFarm.Web.WebBrowser.CreateHttpHandlerDelegate = IApplication.CreateHttpHandler;
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }
}
