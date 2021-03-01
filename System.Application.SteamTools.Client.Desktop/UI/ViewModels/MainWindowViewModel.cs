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
        public GameListPageViewModel GameListPage { get; }
        public SteamAccountPageViewModel SteamAccountPage { get; }


        public IList<TabItemViewModel> TabItems { get; set; }



        public MainWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark;

            this.TabItems = new List<TabItemViewModel>
            {
                (this.StartPage = new StartPageViewModel().AddTo(this)),
                (this.SteamAccountPage = new SteamAccountPageViewModel().AddTo(this)),
                (this.GameListPage = new GameListPageViewModel().AddTo(this)),


                
				#region SystemTab
                SettingsPageViewModel.Instance,
                AboutPageViewModel.Instance,
#if DEBUG
				new DebugPageViewModel().AddTo(this),
#endif
				#endregion
            };

            this.SelectedItem = this.TabItems.First();
        }
    }
}
