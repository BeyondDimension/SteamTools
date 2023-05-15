namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
sealed class Plugin : PluginBase<Plugin>
{
    const string moduleName = "ArchiSteamFarmPlus";

    public override string Name => moduleName;

    public override IEnumerable<TabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel()
        {
            ResourceKeyOrName = "ArchiSteamFarmPlus",
            PageType = null,
            IsResourceGet = true,
            IconKey = "ASF",
        };
    }

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
