using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using MetroTrilithon.Mvvm;
using Livet;

namespace SteamTools.ViewModels
{
    public class MainWindowViewModel : Livet.ViewModel
    {
        // ----- Tab items
        public WelcomePageViewModel WelcomePage { get; }
        public ChangeSteamAccountPageModel AccountPage { get; }
        public LocalAuthPageModel LocalAuthPage { get; }
        public SettingsPageViewModel SettingsPage { get; }
        public CommunityProxyPageViewModel CommunityProxyPage { get; }

        public IList<TabItemViewModel> TabItems { get; set; }
        public IList<TabItemViewModel> SystemTabItems { get; set; }

        #region SelectedItem 変更通知

        private TabItemViewModel _SelectedItem;

        public TabItemViewModel SelectedItem
        {
            get { return this._SelectedItem; }
            set
            {
                if (this._SelectedItem != value)
                {
                    this._SelectedItem = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region NotificationNum 変更通知
        private int _NotificationNum = 0;
        public int NotificationNum
        {
            get { return this._NotificationNum; }
            set
            {
                if (this._NotificationNum != value)
                {
                    this._NotificationNum = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        public MainWindowViewModel()
        {
            this.TabItems = new List<TabItemViewModel>
            {
                (this.WelcomePage = new WelcomePageViewModel(this).AddTo(this)),
                (this.AccountPage = new ChangeSteamAccountPageModel().AddTo(this)),
                (this.LocalAuthPage = new LocalAuthPageModel().AddTo(this)),
                (this.CommunityProxyPage = new CommunityProxyPageViewModel().AddTo(this)),
                //(this.Expeditions = new ExpeditionsViewModel(this.Fleets).AddTo(this)),
            };
            this.SystemTabItems = new List<TabItemViewModel>
            {
                SettingsPageViewModel.Instance,
				#region DEBUG
#if DEBUG
				new DebugPageViewModel().AddTo(this),
#endif
				#endregion
			};
            this.SelectedItem = this.TabItems.FirstOrDefault();
        }
    }
}
