using BD.WTTS.Properties;
using BD.WTTS.UI.Views.Pages;
using BD.WTTS.UI.Views.Windows;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    const string moduleName = AssemblyInfo.SteamIdleCard;

    public override Guid Id => Guid.Parse(AssemblyInfo.SteamIdleCard);

    public override string Name => Strings.SteamIdleCard;

    public sealed override string UniqueEnglishName => moduleName;

    public sealed override string Description => "Steam 游戏空闲挂卡，支持多种算法逻辑和卡片价格展示";

    protected sealed override string? AuthorOriginalString => null;

    public sealed override object? Icon => new MemoryStream(new byte[1024]);

    public override IEnumerable<TabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel()
        {
            ResourceKeyOrName = nameof(Strings.GameList),
            PageType = typeof(MainFramePage),
            IsResourceGet = true,
            IconKey = Icon,
        };
    }

    public override async ValueTask OnCommandRun(params string[] commandParams)
    {
        await ValueTask.CompletedTask;
    }

    public override void ConfigureDemandServices(IServiceCollection services, Startup startup)
    {
        services.AddSteamAccountService(c => new SocketsHttpHandler() { CookieContainer = c });
        services.AddSteamIdleCardService(c => new SocketsHttpHandler() { CookieContainer = c });
    }
}