using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Text;
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

#pragma warning disable CS8618 // SelectedItem不会为null
        public MainWindowViewModel() : base()
#pragma warning restore CS8618 // 
        {
            Title = ThisAssembly.AssemblyTrademark;

            OpenUserMenu = ReactiveCommand.Create(() => { IsOpenUserMenu = true; });

            this.TabItems = new List<TabItemViewModel>
            {
                (this.StartPage = new StartPageViewModel().AddTo(this)),
                (this.CommunityProxyPage = new CommunityProxyPageViewModel().AddTo(this)),
                (this.ProxyScriptPage = new ProxyScriptManagePageViewModel().AddTo(this)),
                (this.SteamAccountPage = new SteamAccountPageViewModel().AddTo(this)),
                (this.GameListPage = new GameListPageViewModel().AddTo(this)),
                (this.LocalAuthPage = new LocalAuthPageViewModel().AddTo(this)),
                (this.SteamIdlePage = new SteamIdlePageViewModel().AddTo(this)),
                (this.ASFPage = new ArchiSteamFarmPlusPageViewModel().AddTo(this)),
                (this.GameRelatedPage = new GameRelatedPageViewModel().AddTo(this)),
                (this.OtherPlatformPage = new OtherPlatformPageViewModel().AddTo(this)),
                
				#region SystemTab
                SettingsPageViewModel.Instance,
                AboutPageViewModel.Instance,
#if DEBUG
				new DebugPageViewModel().AddTo(this),
                new DebugWebViewPageViewModel().AddTo(this),
#endif
				#endregion
            };

            //#if DEBUG
            //            if (AppHelper.Current.IsCefInitComplete)
            //            {
            //                TabItems.Add(new DebugWebViewPageViewModel().AddTo(this));
            //            }
            //#endif

            this.SelectedItem = this.TabItems.First();

            Task.Run(Initialize).ForgetAndDispose();
        }

        public async void Initialize()
        {
            SteamConnectService.Current.Initialize();
            ProxyService.Current.Initialize();
            AuthService.Current.Initialize();

            if (!this.IsInitialized)
            {
                Parallel.ForEach(TabItems, item =>
                {
                    //if (item == GameListPage)
                    //    return;
                    item.Initialize();
                });
                this.IsInitialized = true;
            }

            await Task.CompletedTask;
        }
    }
}
