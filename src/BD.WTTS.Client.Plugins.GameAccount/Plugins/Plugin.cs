using BD.WTTS.UI.Views.Pages;
using System;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    const string moduleName = AssemblyInfo.GameAccount;

    public override Guid Id => Guid.Parse(AssemblyInfo.GameAccountId);

    public sealed override string Name => Strings.UserFastChange;

    public sealed override string UniqueEnglishName => moduleName;

    public sealed override string Description => "可支持自行添加多平台账号快速切换功能，Steam 可自动读取账号信息，其它平台请手动添加账号信息。";

    public sealed override string Author => "Steam++ 官方";

    public sealed override string? Icon => "avares://BD.WTTS.Client.Plugins.GameAccount/UI/Assets/userswitcher.ico";

    public override IEnumerable<TabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel()
        {
            ResourceKeyOrName = nameof(Strings.UserFastChange),
            PageType = typeof(GameAccountPage),
            IsResourceGet = true,
            IconKey = Icon,
        };
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {
        services.AddSingleton<IPartialGameAccountSettings>(s =>
            s.GetRequiredService<IOptionsMonitor<GameAccountSettings_>>().CurrentValue);

        services.AddSingleton<IPlatformSwitcher, BasicPlatformSwitcher>()
                .AddSingleton<IPlatformSwitcher, SteamPlatformSwitcher>();
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }

    public override IEnumerable<(Action<IServiceCollection>? @delegate, bool isInvalid, string name)>? GetConfiguration(bool directoryExists)
    {
        yield return GetConfiguration<GameAccountSettings_>(directoryExists);
    }
}
