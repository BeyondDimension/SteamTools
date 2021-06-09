using ReactiveUI;
using System.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class MainWindowViewModel : WindowViewModel
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

        readonly Dictionary<Type, Lazy<TabItemViewModel>> mTabItems = new();
        public IEnumerable<TabItemViewModel> TabItems => mTabItems.Values.Select(x => x.Value);

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

            //AddTabItem<StartPageViewModel>();
            AddTabItem<CommunityProxyPageViewModel>();
            AddTabItem<ProxyScriptManagePageViewModel>();
            AddTabItem<SteamAccountPageViewModel>();
            AddTabItem<GameListPageViewModel>();
            AddTabItem<LocalAuthPageViewModel>();
            AddTabItem<ArchiSteamFarmPlusPageViewModel>();
            //AddTabItem<SteamIdlePageViewModel>();
            if (DI.Platform == Platform.Windows)
                AddTabItem<GameRelatedPageViewModel>();
            //AddTabItem<OtherPlatformPageViewModel>();

            AddTabItem(() => SettingsPageViewModel.Instance);
            AddTabItem(() => AboutPageViewModel.Instance);

            if (AppHelper.EnableDevtools)
            {
                AddTabItem<DebugPageViewModel>();
                if (AppHelper.IsSystemWebViewAvailable)
                {
                    AddTabItem<DebugWebViewPageViewModel>();
                }
            }

            _SelectedItem = TabItems.First();

            Task.Run(Initialize).ForgetAndDispose();

            //this.WhenAnyValue(x => x.SelectedItem)
            //    .Subscribe(x =>
            //    {
            //        Task.Run(x.Activation).ForgetAndDispose();
            //    });
        }

        public void Initialize()
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
        }

        void AddTabItem<TabItemVM>() where TabItemVM : TabItemViewModel, new()
        {
            Lazy<TabItemViewModel> value = new(() => new TabItemVM().AddTo(this));
            mTabItems.Add(typeof(TabItemVM), value);
        }

        void AddTabItem<TabItemVM>(Func<TabItemVM> func) where TabItemVM : TabItemViewModel
        {
            Lazy<TabItemViewModel> value = new(func);
            mTabItems.Add(typeof(TabItemVM), value);
        }

        TabItemVM GetTabItemVM<TabItemVM>() where TabItemVM : TabItemViewModel => (TabItemVM)mTabItems[typeof(TabItemVM)].Value;
    }
}