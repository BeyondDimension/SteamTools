using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class SteamAccountPageViewModel
    {
        readonly ISteamService steamService = ISteamService.Instance;
        //readonly IHttpService httpService = IHttpService.Instance;
        //readonly ISteamworksWebApiService webApiService = ISteamworksWebApiService.Instance;

        public SteamAccountPageViewModel()
        {
            this.WhenAnyValue(x => x.SteamUsers)
                  .Subscribe(s => this.RaisePropertyChanged(nameof(IsUserEmpty)));
            LoginAccountCommand = ReactiveCommand.Create(LoginNewSteamAccount);
            RefreshCommand = ReactiveCommand.Create(SteamConnectService.Current.RefreshSteamUsers);

            ShareManageCommand = ReactiveCommand.Create(OpenShareManageWindow);
            OpenBrowserCommand = ReactiveCommand.CreateFromTask<string>(x => Browser2.OpenAsync(x));

            //MenuItems = new ObservableCollection<MenuItemViewModel>()
            //{
            //    new MenuItemViewModel (nameof(AppResources.Refresh))
            //                { IconKey="RefreshDrawing" , Command = RefreshCommand},
            //    new MenuItemSeparator (),
            //    new MenuItemViewModel(nameof(AppResources.UserChange_LoginNewAccount))
            //                { IconKey="SteamDrawing", Command=LoginAccountCommand },
            //    new MenuItemViewModel(nameof(AppResources.AccountChange_Title)){
            //          IconKey ="ChannelShareDrawing", Command = ShareManageCommand },
            //    //new MenuItemViewModel(nameof(AppResources.UserChange_RemarkReplaceName)){
            //    //      IconKey ="EditDrawing", Command = ShareManageCommand },
            //};

            SteamConnectService.Current.SteamUsers
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<SteamUser>.Descending(x => x.LastLoginTime))
                .Bind(out _SteamUsers)
                .Subscribe(_ =>
                {
                    Initialize();
                });
        }

        public ReactiveCommand<Unit, Unit> LoginAccountCommand { get; }

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

        public ReactiveCommand<Unit, Unit> ShareManageCommand { get; }

        public ReactiveCommand<string, Unit> OpenBrowserCommand { get; }

        public void OpenShareManageWindow()
        {
            IWindowManager.Instance.Show(CustomWindow.ShareManage, new ShareManageViewModel(), string.Empty, ResizeMode.CanResize);
        }

        readonly ReadOnlyObservableCollection<SteamUser> _SteamUsers;

        /// <summary>
        /// Steam 客户端记住的用户列表
        /// </summary>
        public ReadOnlyObservableCollection<SteamUser> SteamUsers => _SteamUsers;

        public bool IsUserEmpty => !SteamUsers.Any_Nullable();

        public override void Initialize()
        {
            #region 加载windows托盘菜单

            if (OperatingSystem2.IsWindows())
            {
                var menus = new List<MenuItemViewModel>();

                foreach (var user in SteamConnectService.Current.SteamUsers.Items)
                {
                    var title = user.SteamNickName ?? user.SteamId64.ToString(CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(user.Remark))
                    {
                        title = user.SteamNickName + "(" + user.Remark + ")";
                    }

                    menus.Add(new MenuItemCustomName(title, AppResources.UserChange_BtnTootlip)
                    {
                        Command = ReactiveCommand.Create(() =>
                        {
                            SteamId_Click(user);

                            INotificationService.Instance.Notify(string.Format(AppResources.UserChange_ChangeUserTip, title), NotificationType.Message);
                        }),
                    });
                }

                MenuItems = new ObservableCollection<MenuItemViewModel>(menus);
            }

            #endregion
        }

        public void SteamId_Click(SteamUser user)
        {
            user.WantsOfflineMode = false;
            ReStartSteamByUser(user);
        }

        public void OfflineModeButton_Click(SteamUser user)
        {
            user.WantsOfflineMode = true;
            ReStartSteamByUser(user);
        }

        private async void ReStartSteamByUser(SteamUser? user = null)
        {
            foreach (var item in SteamUsers.Where(x => x.MostRecent))
                item.MostRecent = false;

            await steamService.ShutdownSteamAsync();

            if (user != null)
            {
                user.MostRecent = true;
                steamService.UpdateLocalUserData(SteamUsers!);
                steamService.SetCurrentUser(user.AccountName ?? string.Empty);
            }
            else
            {
                steamService.SetCurrentUser(string.Empty);
            }

            steamService.StartSteamWithParameter();
        }

        public async void DeleteUserButton_Click(SteamUser user)
        {
            var result = await MessageBox.ShowAsync(AppResources.UserChange_DeleteUserTip, button: MessageBox.Button.OKCancel);
            if (result == MessageBox.Result.OK)
            {
                result = await MessageBox.ShowAsync(AppResources.UserChange_DeleteUserDataTip, button: MessageBox.Button.OKCancel);
                if (result == MessageBox.Result.OK)
                {
                    steamService.DeleteLocalUserData(user, true);
                }
                else
                {
                    steamService.DeleteLocalUserData(user, false);
                }
                SteamConnectService.Current.SteamUsers.Remove(user);
            }
        }

        public async void OpenUserProfileUrl(SteamUser user)
        {
            await Browser2.OpenAsync(user.ProfileUrl);
        }

        public void OpenUserDataFolder(SteamUser user)
        {
            IPlatformService.Instance.OpenFolder(Path.Combine(ISteamService.Instance.SteamDirPath, user.UserdataPath));
        }

        public void LoginNewSteamAccount()
        {
            var result = MessageBox.ShowAsync(AppResources.UserChange_LoginNewAccountTip, button: MessageBox.Button.OKCancel).ContinueWith(s =>
            {
                if (s.Result == MessageBox.Result.OK)
                {
                    ReStartSteamByUser();
                }
            });
        }

        public async void EditRemarkAsync(SteamUser user)
        {
            var value = await TextBoxWindowViewModel.ShowDialogAsync(new()
            {
                Value = user.Remark,
                Title = AppResources.UserChange_EditRemark,
            });
            if (value == null)
                return;
            user.Remark = value;

            if (SteamAccountSettings.AccountRemarks.Value == null)
                SteamAccountSettings.AccountRemarks.Value = new ConcurrentDictionary<long, string?>();
            SteamAccountSettings.AccountRemarks.Value.AddOrUpdate(user.SteamId64, user.Remark, (oldkey, oldvalue) => user.Remark);
            SteamAccountSettings.AccountRemarks.RaiseValueChanged();
        }
    }
}
