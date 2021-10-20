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

        public ReactiveCommand<Unit, Unit> OpenUserMenu { get; }

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
            var adminTag = platformService.IsAdministrator ? (OperatingSystem2.IsWindows ? " (Administrator)" : " (Root)") : string.Empty;
            Title = $"{ThisAssembly.AssemblyTrademark} {RuntimeInformation.ProcessArchitecture} v{ThisAssembly.VersionDisplay} for {DeviceInfo2.OSName}{adminTag}";

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

            FooterTabItems = InitTabItemsWithReturnFooterTabItems();

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