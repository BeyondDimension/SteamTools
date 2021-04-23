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

        public StartPageViewModel StartPage { get; }
        public CommunityProxyPageViewModel CommunityProxyPage { get; }
        public ProxyScriptManagePageViewModel ProxyScriptPage { get; }
        public SteamAccountPageViewModel SteamAccountPage { get; }
        public GameListPageViewModel GameListPage { get; }
        public LocalAuthPageViewModel LocalAuthPage { get; }
        public SteamIdlePageViewModel SteamIdlePage { get; }
        public ArchiSteamFarmPlusPageViewModel ASFPage { get; }
        public GameRelatedPageViewModel GameRelatedPage { get; }
        public OtherPlatformPageViewModel OtherPlatformPage { get; }

        public IReadOnlyList<TabItemViewModel> TabItems { get; set; }

        public MainWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark;

            IUserManager.Instance.OnSignOut += () =>
            {
                IsOpenUserMenu = false;
            };

            OpenUserMenu = ReactiveCommand.Create(() =>
            {
                IsOpenUserMenu = UserService.Current.User != null;
                if (!IsOpenUserMenu)
                {
                    UserService.Current.ShowWindow(CustomWindow.LoginOrRegister);
                }
            });

            TabItems = new List<TabItemViewModel>
            {
                (StartPage = new StartPageViewModel().AddTo(this)),
                (CommunityProxyPage = new CommunityProxyPageViewModel().AddTo(this)),
                (ProxyScriptPage = new ProxyScriptManagePageViewModel().AddTo(this)),
                (SteamAccountPage = new SteamAccountPageViewModel().AddTo(this)),
                (GameListPage = new GameListPageViewModel().AddTo(this)),
                (LocalAuthPage = new LocalAuthPageViewModel().AddTo(this)),
                //(SteamIdlePage = new SteamIdlePageViewModel().AddTo(this)),
                //(ASFPage = new ArchiSteamFarmPlusPageViewModel().AddTo(this)),
                //(GameRelatedPage = new GameRelatedPageViewModel().AddTo(this)),
                //(OtherPlatformPage = new OtherPlatformPageViewModel().AddTo(this)),

				#region SystemTab
                SettingsPageViewModel.Instance,
                AboutPageViewModel.Instance,
#if DEBUG
				//new DebugPageViewModel().AddTo(this),
    //            new DebugWebViewPageViewModel().AddTo(this),
#endif
				#endregion
            };

            _SelectedItem = TabItems.First();

            Task.Run(Initialize).ForgetAndDispose();
        }

        public void Initialize()
        {
            Threading.Thread.CurrentThread.IsBackground = true;
            SteamConnectService.Current.Initialize();
            ProxyService.Current.Initialize();
            AuthService.Current.Initialize();

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
    }
}