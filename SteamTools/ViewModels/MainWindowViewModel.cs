using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using MetroTrilithon.Mvvm;
using Livet;
using SteamTools.Models;
using SteamTool.Core;
using SteamTools.Win32;
using System.Windows;
using System.Windows.Interop;

namespace SteamTools.ViewModels
{
    public class MainWindowViewModel : Livet.ViewModel
    {
        // ----- Tab items
        public WelcomePageViewModel WelcomePage { get; }
        public SwitchSteamAccountPage AccountPage { get; }
        public LocalAuthPageModel LocalAuthPage { get; }
        public AchievementUnlockedPageModel AchievementUnlockedPage { get; }
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

        #region Badge 変更通知
        private int _Badge = 0;
        public int Badge
        {
            get { return this._Badge; }
            set
            {
                if (this._Badge != value)
                {
                    this._Badge = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        private bool _Visible;
        public bool Visible
        {
            get
            {
                return _Visible;
            }
            set
            {
                if (this._Visible != value)
                {
                    this._Visible = value;
                    ChangeWindowVisible();
                }
            }
        }

        public MainWindowViewModel()
        {
            this.Initialize();

            this.TabItems = new List<TabItemViewModel>
            {
                (this.WelcomePage = new WelcomePageViewModel(this).AddTo(this)),
                (this.CommunityProxyPage = new CommunityProxyPageViewModel().AddTo(this)),
                (this.AccountPage = new SwitchSteamAccountPage().AddTo(this)),
                (this.LocalAuthPage = new LocalAuthPageModel().AddTo(this)),
                (this.AchievementUnlockedPage = new AchievementUnlockedPageModel().AddTo(this)),
                //(this.Expeditions = new ExpeditionsViewModel(this.Fleets).AddTo(this)),
            };
            this.SystemTabItems = new List<TabItemViewModel>
            {
                SettingsPageViewModel.Instance,
                AboutPageModel.Instance,
				#region DEBUG
#if DEBUG
				new DebugPageViewModel().AddTo(this),
#endif
				#endregion
			};
            this.SelectedItem = this.TabItems.FirstOrDefault();
        }

        public void Initialize()
        {
            //加载本地steam记住登陆用户
            SteamToolService steamService = SteamToolCore.Instance.Get<SteamToolService>();
            GlobalVariable.Instance.LocalSteamUser = steamService.GetAllUser();
        }

        /// <summary>
        /// 窗口显示关闭
        /// </summary>
        public void ChangeWindowVisible()
        {
            App.Current.MainWindow = App.Current.MainWindow ?? new MainWindow();
            if (Visible)
            {
                App.Current.MainWindow.WindowState = WindowState.Minimized;
                App.Current.MainWindow.Hide();
            }
            else
            {
                App.Current.MainWindow.Show();
                //App.Current.MainWindow.Focus();
                App.Current.MainWindow.WindowState = WindowState.Normal;
                FlashTaskBar.FlashWindow(new WindowInteropHelper(App.Current.MainWindow).Handle);
            }
        }
    }
}
