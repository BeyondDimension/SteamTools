using System.Application.UI.Resx;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
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
            TabItemId.Debug => typeof(DebugPageViewModel),
            _ => throw new ArgumentOutOfRangeException(nameof(tabItemId), tabItemId, null),
        };

        public static TabItemViewModel Create(TabItemId tabItemId) => tabItemId switch
        {
            TabItemId.Settings => SettingsPageViewModel.Instance,
            TabItemId.About => AboutPageViewModel.Instance,
#if DEBUG
            TabItemId.Debug => DebugPageViewModel.Instance,
#endif
            _ => (TabItemViewModel)System.Activator.CreateInstance(GetType(tabItemId))!,
        };
    }

    partial class StartPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.Welcome;

        public override string Name
        {
            get => DisplayName;
        }
    }

    partial class CommunityProxyPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.CommunityFix;

        public override TabItemId Id => TabItemId.CommunityProxy;

        public override string Name
        {
            get => DisplayName;
        }
    }

    partial class ProxyScriptManagePageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.ScriptConfig;

        public override TabItemId Id => TabItemId.ProxyScriptManage;

        public override string Name
        {
            get => DisplayName;
        }
    }

    partial class SteamAccountPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.UserFastChange;

        public override TabItemId Id => TabItemId.SteamAccount;

        public override string Name
        {
            get => DisplayName;
        }
    }

    partial class GameListPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.GameList;

        public override TabItemId Id => TabItemId.GameList;

        public override string Name
        {
            get => DisplayName;
        }
    }

    partial class LocalAuthPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.LocalAuth;

        public override TabItemId Id => TabItemId.LocalAuth;

        public override string Name
        {
            get => DisplayName;
        }
    }

    partial class ArchiSteamFarmPlusPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.ArchiSteamFarmPlus;

        public override TabItemId Id => TabItemId.ArchiSteamFarmPlus;

        public override string Name
        {
            get => DisplayName;
        }
    }

    partial class SettingsPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.Settings;

        public override TabItemId Id => TabItemId.Settings;

        public override string Name
        {
            get => DisplayName;
        }
    }

    partial class AboutPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.About;

        public override TabItemId Id => TabItemId.About;

        public override string Name
        {
            get => DisplayName;
        }
    }

    partial class SteamIdlePageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.IdleCard;

        public override string Name
        {
            get => DisplayName;
        }
    }

    partial class OtherPlatformPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.OtherGamePlaform;

        public override string Name
        {
            get => DisplayName;
        }
    }

#if DEBUG
    partial class DebugPageViewModel : TabItemViewModel
    {
        public const string DisplayName = "Debug";

        public override TabItemId Id => TabItemId.Debug;

        public override string Name
        {
            get => DisplayName;
        }
    }
#endif

    partial class GameRelatedPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.GameRelated;

        public override string Name
        {
            get => DisplayName;
        }
    }
}
