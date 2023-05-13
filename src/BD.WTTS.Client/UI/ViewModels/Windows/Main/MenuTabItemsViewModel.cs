using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

#if !WTTS_PLUGIN
/// <summary>
/// 起始页/主页的菜单项视图模型
/// </summary>
public sealed class HomeMenuTabItemViewModel : TabItemViewModel, IReadOnlyStaticDisplayName
{
    public static string? DisplayName => resourceManager.GetString("Welcome");

    public override string? Name => DisplayName;
}
#endif

#if WTTS_PLUGIN_ACCELERATOR
/// <summary>
/// 网络加速的菜单项视图模型
/// </summary>
/// 
public sealed class AcceleratorMenuTabItemViewModel : TabItemViewModel
{
    public static string? DisplayName => resourceManager.GetString("CommunityFix");

    public override string? Name => DisplayName;
}

/// <summary>
/// 网络加速脚本/脚本配置的菜单项视图模型
/// </summary>
public sealed class AcceleratorScriptMenuTabItemViewModel : TabItemViewModel
{
    public override string? Name => resourceManager.GetString("ScriptConfig");
}
#endif

#if WTTS_PLUGIN_GAMEACCOUNT
/// <summary>
/// Steam 账号列表的菜单项视图模型
/// </summary>
public sealed class GameAccountMenuTabItemViewModel : TabItemViewModel, IReadOnlyStaticDisplayName
{
    public static string DisplayName => AppResources.UserFastChange;

    public override string Name => DisplayName;
}
#endif

#if WTTS_PLUGIN_GAMELIST
/// <summary>
/// Steam 游戏库存的菜单项视图模型
/// </summary>
public sealed class GameListMenuTabItemViewModel : TabItemViewModel, IReadOnlyStaticDisplayName
{
    public static string DisplayName => AppResources.GameList;

    public override string Name => DisplayName;
}
#endif

#if WTTS_PLUGIN_AUTHENTICATOR
/// <summary>
/// Authenticator 令牌的菜单项视图模型
/// </summary>
public sealed class AuthenticatorMenuTabItemViewModel : TabItemViewModel, IReadOnlyStaticDisplayName
{
    public static string DisplayName => AppResources.LocalAuth;

    public override string Name => DisplayName;
}
#endif

#if WTTS_PLUGIN_ASFPLUS
/// <summary>
/// ArchiSteamFarm 的菜单项视图模型
/// </summary>
public sealed class ArchiSteamFarmPlusMenuTabItemViewModel : TabItemViewModel, IReadOnlyStaticDisplayName
{
    public static string DisplayName => AppResources.ArchiSteamFarmPlus;

    public override string Name => DisplayName;
}
#endif

#if !WTTS_PLUGIN
/// <summary>
/// Settings 设置项的菜单项视图模型
/// </summary>
public sealed class SettingsMenuTabItemViewModel : TabItemViewModel, IReadOnlyStaticDisplayName
{
    public static string DisplayName => AppResources.Settings;

    public override string Name => DisplayName;
}

/// <summary>
/// 关于的菜单项视图模型
/// </summary>
public sealed class AboutMenuTabItemViewModel : TabItemViewModel, IReadOnlyStaticDisplayName
{
    public static string DisplayName => AppResources.About;

    public override string Name => DisplayName;
}

#if DEBUG
public sealed class DebugMenuTabItemViewModel : TabItemViewModel
{
    public const string DisplayName = "Debug";

    public override string Name => DisplayName;
}
#endif
#endif

#if WTTS_PLUGIN_GAMETOOLS
/// <summary>
/// 游戏相关的工具的菜单项视图模型
/// </summary>
public sealed class GameToolsMenuTabItemViewModel : TabItemViewModel, IReadOnlyStaticDisplayName
{
    public static string DisplayName => AppResources.GameRelated;

    public override string Name => DisplayName;
}
#endif