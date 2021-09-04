using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    partial class TabItemViewModel
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

        public virtual TabItemId Id { get; }

        public abstract string Name { get; protected set; }
    }

#if !__MOBILE__
    partial class CommunityProxyPageViewModel : TabItemViewModel
    {
        public override TabItemId Id => TabItemId.CommunityProxy;

        public override string Name
        {
            get => AppResources.CommunityFix;
            protected set { throw new NotImplementedException(); }
        }
    }

    partial class ProxyScriptManagePageViewModel : TabItemViewModel
    {
        public override TabItemId Id => TabItemId.ProxyScriptManage;

        public override string Name
        {
            get => AppResources.ScriptConfig;
            protected set { throw new NotImplementedException(); }
        }
    }

    partial class SteamAccountPageViewModel : TabItemViewModel
    {
        public override TabItemId Id => TabItemId.SteamAccount;

        public override string Name
        {
            get => AppResources.UserFastChange;
            protected set { throw new NotImplementedException(); }
        }
    }

    partial class GameListPageViewModel : TabItemViewModel
    {
        public override TabItemId Id => TabItemId.GameList;

        public override string Name
        {
            get => AppResources.GameList;
            protected set { throw new NotImplementedException(); }
        }
    }
#endif

    partial class LocalAuthPageViewModel : TabItemViewModel
    {
        public override TabItemId Id => TabItemId.LocalAuth;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotImplementedException(); }
        }
    }

#if !__MOBILE__
    partial class ArchiSteamFarmPlusPageViewModel : TabItemViewModel
    {
        public override TabItemId Id => TabItemId.ArchiSteamFarmPlus;

        public override string Name
        {
            get => AppResources.ArchiSteamFarmPlus;
            protected set { throw new NotImplementedException(); }
        }
    }
#endif

    partial class SettingsPageViewModel : TabItemViewModel
    {
        public override TabItemId Id => TabItemId.Settings;

        public override string Name
        {
            get => AppResources.Settings;
            protected set { throw new NotImplementedException(); }
        }

        public static SettingsPageViewModel Instance { get; } = new();
    }

    partial class AboutPageViewModel : TabItemViewModel
    {
        public override TabItemId Id => TabItemId.About;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotSupportedException(); }
        }

#if !__MOBILE__
        public static AboutPageViewModel Instance { get; } = new();
#endif
    }
}