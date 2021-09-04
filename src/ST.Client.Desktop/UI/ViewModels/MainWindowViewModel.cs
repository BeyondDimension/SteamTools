using ReactiveUI;
using System.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Threading.Tasks;

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

        public MainWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark;

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

            //Task.Run(Initialize).ForgetAndDispose();

            //this.WhenAnyValue(x => x.SelectedItem)
            //    .Subscribe(x =>
            //    {
            //        Task.Run(x.Activation).ForgetAndDispose();
            //    });
        }

        public override async void Initialize()
        {
            await Task.Run(() =>
            {
                Threading.Thread.CurrentThread.IsBackground = true;
                ProxyService.Current.Initialize();
                SteamConnectService.Current.Initialize();

                if (!IsInitialized)
                {
                    Parallel.ForEach(TabItems, item =>
                    {
                        item.Initialize();
                        //Task.Run(item.Initialize).ForgetAndDispose();
                    });
                    IsInitialized = true;
                }
            });
        }
    }
}