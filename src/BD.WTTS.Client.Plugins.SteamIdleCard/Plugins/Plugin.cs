using BD.WTTS.Properties;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    const string moduleName = AssemblyInfo.SteamIdleCard;

    public override Guid Id => Guid.Parse(AssemblyInfo.SteamIdleCardId);

    public override string Name => Strings.SteamIdleCard;

    public sealed override string UniqueEnglishName => moduleName;

    public sealed override string Description => "Steam 游戏快速挂卡，需要启动 Steam 客户端并登录对应账号，支持多种算法逻辑挂卡，掉卡速度更快。";

    protected sealed override string? AuthorOriginalString => null;

    public sealed override object? Icon => Resources.card;

    public override IEnumerable<MenuTabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel(this, nameof(Strings.SteamIdleCard))
        {
            PageType = typeof(IdleCardPage),
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

    public override IEnumerable<(Action<IServiceCollection>? @delegate, bool isInvalid, string name)>? GetConfiguration(bool directoryExists)
    {
        yield return GetConfiguration<SteamIdleSettings_>(directoryExists);
    }
}