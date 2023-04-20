// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels.Abstractions;

public abstract class TabItemViewModel : TabItemViewModel<TabItemViewModel.TabItemId>
{
    public enum TabItemId : byte
    {
        Accelerator = 1,
        ProxyScriptManage,
        AccountSwitch,
        GameList,
        LocalAuth,
        ArchiSteamFarmPlus,
        GameTools,
        Settings,
        About,
        Start,
#if DEBUG
        Debug = byte.MaxValue,
#endif
        CommunityProxy = Accelerator,
        SteamAccount = AccountSwitch,
        GameRelated = GameTools,
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(TabItemId tabItemId) => tabItemId switch
    {
        TabItemId.Accelerator => CommunityProxyPageViewModel.DisplayName,
        TabItemId.ProxyScriptManage => ProxyScriptManagePageViewModel.DisplayName,
        TabItemId.AccountSwitch => SteamAccountPageViewModel.DisplayName,
        TabItemId.GameList => GameListPageViewModel.DisplayName,
        TabItemId.LocalAuth => LocalAuthPageViewModel.DisplayName,
        TabItemId.ArchiSteamFarmPlus => ArchiSteamFarmPlusPageViewModel.DisplayName,
        TabItemId.GameTools => GameRelatedPageViewModel.DisplayName,
        TabItemId.Settings => SettingsPageViewModel.DisplayName,
        TabItemId.About => AboutPageViewModel.DisplayName,
        TabItemId.Start => StartPageViewModel.DisplayName,
#if DEBUG
        TabItemId.Debug => DebugPageViewModel.DisplayName,
#endif
        _ => "",
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Type GetType(TabItemId tabItemId) => tabItemId switch
    {
        TabItemId.Accelerator => typeof(CommunityProxyPageViewModel),
        TabItemId.ProxyScriptManage => typeof(ProxyScriptManagePageViewModel),
        TabItemId.AccountSwitch => typeof(SteamAccountPageViewModel),
        TabItemId.GameList => typeof(GameListPageViewModel),
        TabItemId.LocalAuth => typeof(LocalAuthPageViewModel),
        TabItemId.ArchiSteamFarmPlus => typeof(ArchiSteamFarmPlusPageViewModel),
        TabItemId.GameTools => typeof(GameRelatedPageViewModel),
        TabItemId.Settings => typeof(SettingsPageViewModel),
        TabItemId.About => typeof(AboutPageViewModel),
        TabItemId.Start => typeof(StartPageViewModel),
#if DEBUG
        TabItemId.Debug => typeof(DebugPageViewModel),
#endif
        _ => throw new ArgumentOutOfRangeException(nameof(tabItemId), tabItemId, null),
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TabItemViewModel Create(TabItemId tabItemId) => tabItemId switch
    {
        TabItemId.Settings => Ioc.Get<SettingsPageViewModel>(),
        TabItemId.About => Ioc.Get<AboutPageViewModel>(),
#if DEBUG
        TabItemId.Debug => Ioc.Get<DebugPageViewModel>(),
#endif
        _ => CreateCore(tabItemId),
    };

    static TabItemViewModel CreateCore(TabItemId tabItemId)
    {
        var tabItemViewModelType = GetType(tabItemId);
        tabItemViewModelType.ThrowIsNull();
        if (Ioc.Get_Nullable(tabItemViewModelType)
            is TabItemViewModel tabItemViewModel)
            return tabItemViewModel;
        //else if (System.Activator.CreateInstance(tabItemViewModelType)
        //    is TabItemViewModel tabItemViewModel2)
        //    return tabItemViewModel2;
        throw new ArgumentOutOfRangeException(nameof(tabItemId), tabItemId, null);
    }
}