using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Properties;
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

        #endregion

        public StartPageViewModel StartPage { get; }
        public CommunityProxyPageViewModel CommunityProxyPage { get; }
        public SteamAccountPageViewModel SteamAccountPage { get; }
        public GameListPageViewModel GameListPage { get; }
        public LocalAuthPageViewModel LocalAuthPage { get; }
        public SteamIdlePageViewModel SteamIdlePage { get; }
        public ArchiSteamFarmPlusPageViewModel ASFPage { get; }
        public GameRelatedPageViewModel GameRelatedPage { get; }
        public OtherPlatformPageViewModel OtherPlatformPage { get; }

        public IList<TabItemViewModel> TabItems { get; set; }



#pragma warning disable CS8618 // SelectedItem不会为null
        public MainWindowViewModel() : base()
#pragma warning restore CS8618 // 
        {
            Title = ThisAssembly.AssemblyTrademark;

            this.TabItems = new List<TabItemViewModel>
            {
                (this.StartPage = new StartPageViewModel().AddTo(this)),
                (this.CommunityProxyPage = new CommunityProxyPageViewModel().AddTo(this)),
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
#endif
				#endregion
            };

            this.SelectedItem = this.TabItems.First();

            if (ViewModelBase.IsInDesignMode) 
            {
                return;
            }
            Task.Run(Initialize).ContinueWith(s =>
            {
                Log.Error(nameof(MainWindowViewModel), s.Exception, nameof(Initialize) + "Action Error");
            }, TaskContinuationOptions.OnlyOnFaulted)
            .ContinueWith(s => s.Dispose());
        }


        public async void Initialize()
        {
            if (!this.IsInitialized)
            {
                foreach (var item in this.TabItems)
                {
                    if (item == GameListPage)
                        continue;
                    await item.Initialize();
                }
                this.IsInitialized = true;
            }
        }
    }
}
