using ReactiveUI;
using System.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class MainWindowViewModel : WindowViewModel
    {
        #region 更改通知

        bool mTopmost;
        public bool Topmost
        {
            get => mTopmost;
            set => this.RaiseAndSetIfChanged(ref mTopmost, value);
        }

        private TabItemViewModel _SelectedItem;
        public TabItemViewModel SelectedItem
        {
            get => _SelectedItem;
            set => this.RaiseAndSetIfChanged(ref _SelectedItem, value);
        }

        bool _IsOpenUserMenu;
        public bool IsOpenUserMenu
        {
            get => _IsOpenUserMenu;
            set => this.RaiseAndSetIfChanged(ref _IsOpenUserMenu, value);
        }

        public ReactiveCommand<Unit, Unit>? OpenUserMenu { get; }

        #endregion

        public StartPageViewModel StartPage => GetTabItemVM<StartPageViewModel>();

        public CommunityProxyPageViewModel CommunityProxyPage => GetTabItemVM<CommunityProxyPageViewModel>();

        public ProxyScriptManagePageViewModel ProxyScriptPage => GetTabItemVM<ProxyScriptManagePageViewModel>();

        public SteamAccountPageViewModel SteamAccountPage => GetTabItemVM<SteamAccountPageViewModel>();

        public GameListPageViewModel GameListPage => GetTabItemVM<GameListPageViewModel>();

        public LocalAuthPageViewModel LocalAuthPage => GetTabItemVM<LocalAuthPageViewModel>();

        public SteamIdlePageViewModel SteamIdlePage => GetTabItemVM<SteamIdlePageViewModel>();

        public ArchiSteamFarmPlusPageViewModel ASFPage => GetTabItemVM<ArchiSteamFarmPlusPageViewModel>();

        public GameRelatedPageViewModel GameRelatedPage => GetTabItemVM<GameRelatedPageViewModel>();

        public OtherPlatformPageViewModel OtherPlatformPage => GetTabItemVM<OtherPlatformPageViewModel>();

        protected static readonly IPlatformService platformService = IPlatformService.Instance;
        public MainWindowViewModel()
        {
            var useAvalonia = OperatingSystem2.Application.UseAvalonia;
            if (useAvalonia)
            {
                var adminTag = platformService.IsAdministrator ? (OperatingSystem2.IsWindows ? " (Administrator)" : " (Root)") : string.Empty;
                Title = $"{ThisAssembly.AssemblyTrademark} {RuntimeInformation.ProcessArchitecture.ToString().ToLower()} v{ThisAssembly.VersionDisplay} for {DeviceInfo2.OSName}{adminTag}";

                IUserManager.Instance.OnSignOut += () =>
                {
                    IsOpenUserMenu = false;
                };

                OpenUserMenu = ReactiveCommand.Create(() =>
                {
                    IsOpenUserMenu = UserService.Current.IsAuthenticated;
                    if (!IsOpenUserMenu)
                    {
                        UserService.Current.ShowWindow(CustomWindow.LoginOrRegister);
                    }
                });


                FooterTabItems = new List<TabItemViewModel>
                {
                    SettingsPageViewModel.Instance,
                    AboutPageViewModel.Instance,
                };
            }

            #region InitTabItems

            if (useAvalonia)
            {
                //AddTabItem<StartPageViewModel>();
                AddTabItem<CommunityProxyPageViewModel>();
                AddTabItem<ProxyScriptManagePageViewModel>();
                AddTabItem<SteamAccountPageViewModel>();
                AddTabItem<GameListPageViewModel>();
            }
            AddTabItem<LocalAuthPageViewModel>();
            var isVersion_2_5_OR_GREATER =
#if DEBUG
                true;
#else
                new Version(global::System.Properties.ThisAssembly.Version) >= new Version(2, 5);
#endif

            if (isVersion_2_5_OR_GREATER)
            {
                AddTabItem<ArchiSteamFarmPlusPageViewModel>();
            }

            //AddTabItem<SteamIdlePageViewModel>();
#if !TRAY_INDEPENDENT_PROGRAM
            if (OperatingSystem2.IsWindows && useAvalonia)
                AddTabItem<GameRelatedPageViewModel>();
#endif
            //AddTabItem<OtherPlatformPageViewModel>();

#if !TRAY_INDEPENDENT_PROGRAM
            if (IApplication.EnableDevtools && useAvalonia)
            {
                AddTabItem<DebugPageViewModel>();
                //FooterTabItems.Add(new DebugPageViewModel().AddTo(this));

                //if (AppHelper.IsSystemWebViewAvailable)
                //{
                //    AddTabItem<DebugWebViewPageViewModel>();
                //}
            }
#endif

            #endregion

            _SelectedItem = TabItems.First();
        }

        public override void Initialize()
        {
            Task.Run(() =>
            {
                Threading.Thread.CurrentThread.IsBackground = true;
                if (!IsInitialized)
                {
                    ProxyService.Current.Initialize();
                    SteamConnectService.Current.Initialize();

                    Parallel.ForEach(TabItems, item =>
                    {
                        item.Initialize();
                        //Task.Run(item.Initialize).ForgetAndDispose();
                    });
                    IsInitialized = true;
                }
            }).ForgetAndDispose();
        }
    }
}