using BD.WTTS.Properties;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    const string moduleName = AssemblyInfo.GameTools;

    public override Guid Id => Guid.Parse(AssemblyInfo.GameToolsId);

    public override string Name => Strings.GameRelated;

    public sealed override string UniqueEnglishName => moduleName;

    public sealed override string Description => "通用游戏工具";

    protected sealed override string? AuthorOriginalString => null;

    public sealed override object? Icon => Resources.toolbox; //"avares://BD.WTTS.Client.Plugins.GameTools/UI/Assets/toolbox.ico";

    public override IEnumerable<MenuTabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel(this, nameof(Strings.GameRelated))
        {
            PageType = typeof(GameToolsPage),
            IsResourceGet = true,
            IconKey = Icon,
        };
    }

    public override void ConfigureDemandServices(IServiceCollection services, Startup startup)
    {
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }
}
