using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public sealed partial class StartPageViewModel : TabItemViewModel
{
    public static string DisplayName => AppResources.Welcome;

    public override string Name => DisplayName;
}

public sealed partial class CommunityProxyPageViewModel : TabItemViewModel
{
    public static string DisplayName => AppResources.CommunityFix;

    public override TabItemId Id => TabItemId.CommunityProxy;

    public override string Name => DisplayName;
}

public sealed partial class ProxyScriptManagePageViewModel : TabItemViewModel
{
    public static string DisplayName => AppResources.ScriptConfig;

    public override TabItemId Id => TabItemId.ProxyScriptManage;

    public override string Name => DisplayName;
}

public sealed partial class SteamAccountPageViewModel : TabItemViewModel
{
    public static string DisplayName => AppResources.UserFastChange;

    public override TabItemId Id => TabItemId.SteamAccount;

    public override string Name => DisplayName;
}

public sealed partial class GameListPageViewModel : TabItemViewModel
{
    public static string DisplayName => AppResources.GameList;

    public override TabItemId Id => TabItemId.GameList;

    public override string Name => DisplayName;
}

public sealed partial class LocalAuthPageViewModel : TabItemViewModel
{
    public static string DisplayName => AppResources.LocalAuth;

    public override TabItemId Id => TabItemId.LocalAuth;

    public override string Name => DisplayName;
}

public sealed partial class ArchiSteamFarmPlusPageViewModel : TabItemViewModel
{
    public static string DisplayName => AppResources.ArchiSteamFarmPlus;

    public override TabItemId Id => TabItemId.ArchiSteamFarmPlus;

    public override string Name => DisplayName;
}

public sealed partial class SettingsPageViewModel : TabItemViewModel
{
    public static string DisplayName => AppResources.Settings;

    public override TabItemId Id => TabItemId.Settings;

    public override string Name => DisplayName;
}

public sealed partial class AboutPageViewModel : TabItemViewModel
{
    public static string DisplayName => AppResources.About;

    public override TabItemId Id => TabItemId.About;

    public override string Name => DisplayName;
}

public sealed partial class SteamIdlePageViewModel : TabItemViewModel
{
    public static string DisplayName => AppResources.IdleCard;

    public override string Name => DisplayName;
}

public sealed partial class OtherPlatformPageViewModel : TabItemViewModel
{
    public static string DisplayName => AppResources.OtherGamePlaform;

    public override string Name => DisplayName;
}

#if DEBUG
public sealed partial class DebugPageViewModel : TabItemViewModel
{
    public const string DisplayName = "Debug";

    public override TabItemId Id => TabItemId.Debug;

    public override string Name => DisplayName;
}
#endif

public sealed partial class GameRelatedPageViewModel : TabItemViewModel
{
    public static string DisplayName => AppResources.GameRelated;

    public override string Name => DisplayName;
}