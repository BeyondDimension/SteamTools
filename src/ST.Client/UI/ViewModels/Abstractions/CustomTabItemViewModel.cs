using System;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Text;

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
        }
    }
}

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels.Abstractions
{
    abstract partial class StartPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.Welcome;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotImplementedException(); }
        }
    }

    abstract partial class CommunityProxyPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.CommunityFix;

        public override TabItemId Id => TabItemId.CommunityProxy;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotImplementedException(); }
        }
    }

    abstract partial class ProxyScriptManagePageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.ScriptConfig;

        public override TabItemId Id => TabItemId.ProxyScriptManage;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotImplementedException(); }
        }
    }

    abstract partial class SteamAccountPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.UserFastChange;

        public override TabItemId Id => TabItemId.SteamAccount;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotImplementedException(); }
        }
    }

    abstract partial class GameListPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.GameList;

        public override TabItemId Id => TabItemId.GameList;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotImplementedException(); }
        }
    }

    abstract partial class LocalAuthPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.LocalAuth;

        public override TabItemId Id => TabItemId.LocalAuth;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotImplementedException(); }
        }
    }

    abstract partial class ArchiSteamFarmPlusPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.ArchiSteamFarmPlus;

        public override TabItemId Id => TabItemId.ArchiSteamFarmPlus;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotImplementedException(); }
        }
    }

    abstract partial class SettingsPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.Settings;

        public override TabItemId Id => TabItemId.Settings;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotImplementedException(); }
        }
    }

    abstract partial class AboutPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.About;

        public override TabItemId Id => TabItemId.About;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotSupportedException(); }
        }
    }

    abstract partial class SteamIdlePageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.IdleCard;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotSupportedException(); }
        }
    }

    abstract partial class OtherPlatformPageViewModel : TabItemViewModel
    {
        public static string DisplayName => AppResources.OtherGamePlaform;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotSupportedException(); }
        }
    }

    abstract partial class DebugPageViewModel : TabItemViewModel
    {
        public const string DisplayName = "Debug";

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotSupportedException(); }
        }
    }
}
