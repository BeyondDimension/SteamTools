// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels.Abstractions;

public abstract class TabItemViewModel : TabItemViewModel<TabItemViewModel.TabItemId>
{
    public enum TabItemId : byte
    {
        CommunityProxy = 1,
        ProxyScriptManage,
        SteamAccount,
        GameList,
        LocalAuth,
        ArchiSteamFarmPlus,
        GameRelated,
        Settings,
        About,
#if DEBUG
        Debug = byte.MaxValue,
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(TabItemId tabItemId) => tabItemId switch
    {
        TabItemId.CommunityProxy => CommunityProxyPageViewModel.DisplayName,
        TabItemId.ProxyScriptManage => ProxyScriptManagePageViewModel.DisplayName,
        TabItemId.SteamAccount => SteamAccountPageViewModel.DisplayName,
        TabItemId.GameList => GameListPageViewModel.DisplayName,
        TabItemId.LocalAuth => LocalAuthPageViewModel.DisplayName,
        TabItemId.ArchiSteamFarmPlus => ArchiSteamFarmPlusPageViewModel.DisplayName,
        TabItemId.GameRelated => GameRelatedPageViewModel.DisplayName,
        TabItemId.Settings => SettingsPageViewModel.DisplayName,
        TabItemId.About => AboutPageViewModel.DisplayName,
#if DEBUG
        TabItemId.Debug => DebugPageViewModel.DisplayName,
#endif
        _ => "",
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Type GetType(TabItemId tabItemId) => tabItemId switch
    {
        TabItemId.CommunityProxy => typeof(CommunityProxyPageViewModel),
        TabItemId.ProxyScriptManage => typeof(ProxyScriptManagePageViewModel),
        TabItemId.SteamAccount => typeof(SteamAccountPageViewModel),
        TabItemId.GameList => typeof(GameListPageViewModel),
        TabItemId.LocalAuth => typeof(LocalAuthPageViewModel),
        TabItemId.ArchiSteamFarmPlus => typeof(ArchiSteamFarmPlusPageViewModel),
        TabItemId.GameRelated => typeof(GameRelatedPageViewModel),
        TabItemId.Settings => typeof(SettingsPageViewModel),
        TabItemId.About => typeof(AboutPageViewModel),
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