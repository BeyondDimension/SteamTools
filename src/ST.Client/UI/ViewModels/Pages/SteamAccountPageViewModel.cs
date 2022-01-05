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
        readonly IHttpService httpService = IHttpService.Instance;
        readonly ISteamworksWebApiService webApiService = ISteamworksWebApiService.Instance;

        public SteamAccountPageViewModel()
        {
            IconKey = nameof(SteamAccountPageViewModel);

            this.WhenAnyValue(x => x.SteamUsers)
                  .Subscribe(s => this.RaisePropertyChanged(nameof(IsUserEmpty)));
            LoginAccountCommand = ReactiveCommand.Create(LoginNewSteamAccount);
            RefreshCommand = ReactiveCommand.Create(Initialize);

            ShareManageCommand = ReactiveCommand.Create(OpenShareManageWindow);
            OpenBrowserCommand = ReactiveCommand.CreateFromTask<string>(Browser2.OpenAsync);

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

            _SteamUsersSourceList = new SourceCache<SteamUser, long>(t => t.SteamId64);

            _SteamUsersSourceList
              .Connect()
              .ObserveOn(RxApp.MainThreadScheduler)
              .Sort(SortExpressionComparer<SteamUser>.Descending(x => x.LastLoginTime))
              .Bind(out _SteamUsers)
              .Subscribe();
        }

        public ReactiveCommand<Unit, Unit> LoginAccountCommand { get; }

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

        public ReactiveCommand<Unit, Unit> ShareManageCommand { get; }
        public ReactiveCommand<string, Unit> OpenBrowserCommand { get; }

        public void OpenShareManageWindow()
        {
            IWindowManager.Instance.Show(CustomWindow.ShareManage, new ShareManageViewModel(), string.Empty, ResizeMode.CanResize);
        }

        readonly SourceCache<SteamUser, long> _SteamUsersSourceList;
        readonly ReadOnlyObservableCollection<SteamUser>? _SteamUsers;
        /// <summary>
        /// Steam 客户端记住的用户列表
        /// </summary>
        public ReadOnlyObservableCollection<SteamUser>? SteamUsers => _SteamUsers;

        public bool IsUserEmpty => !SteamUsers.Any_Nullable();

        public override async void Initialize()
        {
            var list = steamService.GetRememberUserList();

            if (!list.Any_Nullable())
            {
                return;
            }
            _SteamUsersSourceList.AddOrUpdate(list);

            #region 加载备注信息
            IReadOnlyDictionary<long, string?>? accountRemarks = SteamAccountSettings.AccountRemarks.Value;

            MenuItems = new ObservableCollection<MenuItemViewModel>();

            List<(string title, string accountName)>? jumplistData = OperatingSystem2.IsWindows ? new() : null;
            foreach (var user in _SteamUsersSourceList.Items)
            {
                if (accountRemarks?.TryGetValue(user.SteamId64, out var remark) == true &&
                    !string.IsNullOrEmpty(remark))
                    user.Remark = remark;

                if (OperatingSystem2.IsWindows)
                {
                    var title = user.SteamNickName ?? user.SteamId64.ToString(CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(user.Remark))
                    {
                        title = user.SteamNickName + "(" + user.Remark + ")";
                    }

                    MenuItems.Add(new MenuItemCustomName(title, AppResources.UserChange_BtnTootlip)
                    {
                        Command = ReactiveCommand.Create(() =>
                        {
                            SteamId_Click(user);

                            INotificationService.Instance.Notify(string.Format(AppResources.UserChange_ChangeUserTip, title), NotificationType.Message);
                        }),
                    });

                    if (!string.IsNullOrEmpty(user.AccountName)) jumplistData!.Add((title, user.AccountName));
                }
            }

            if (jumplistData.Any_Nullable())
            {
                MainThread2.BeginInvokeOnMainThread(() =>
                {
                    foreach (var (title, accountName) in jumplistData)
                    {
                        IJumpListService.Instance.AddJumpTask(title, IApplication.ProgramPath, IApplication.ProgramPath, "-clt steam -account " + accountName, AppResources.UserChange_BtnTootlip, Name);
                    }
                });
            }

            _SteamUsersSourceList.Refresh();
            #endregion

            #region 通过webapi加载头像图片用户信息
            foreach (var user in _SteamUsersSourceList.Items)
            {
                var temp = await webApiService.GetUserInfo(user.SteamId64);
                if (!string.IsNullOrEmpty(temp.SteamID))
                {
                    user.SteamID = temp.SteamID;
                    user.OnlineState = temp.OnlineState;
                    user.MemberSince = temp.MemberSince;
                    user.VacBanned = temp.VacBanned;
                    user.Summary = temp.Summary;
                    user.PrivacyState = temp.PrivacyState;
                    user.AvatarIcon = temp.AvatarIcon;
                    user.AvatarMedium = temp.AvatarMedium;
                    user.AvatarFull = temp.AvatarFull;
                    user.MiniProfile = temp.MiniProfile;

                    if (user.MiniProfile != null && !string.IsNullOrEmpty(user.MiniProfile.AnimatedAvatar))
                    {
                        user.AvatarStream = httpService.GetImageAsync(user.MiniProfile.AnimatedAvatar, ImageChannelType.SteamAvatars);
                    }
                    else
                    {
                        user.AvatarStream = httpService.GetImageAsync(temp.AvatarFull, ImageChannelType.SteamAvatars);
                    }
                }
            }

            _SteamUsersSourceList.Refresh();
            #endregion

            #region 加载动态头像头像框数据
            //foreach (var item in _SteamUsersSourceList.Items)
            //{
            //    item.MiniProfile = await webApiService.GetUserMiniProfile(item.SteamId3_Int);
            //    var miniProfile = item.MiniProfile;
            //    if (miniProfile != null)
            //    {
            //        if (!string.IsNullOrEmpty(miniProfile.AnimatedAvatar))
            //            item.AvatarStream = httpService.GetImageAsync(miniProfile.AnimatedAvatar, ImageChannelType.SteamAvatars);

            //        if (!string.IsNullOrEmpty(miniProfile.AvatarFrame))
            //            miniProfile.AvatarFrameStream = httpService.GetImageAsync(miniProfile.AvatarFrame, ImageChannelType.SteamAvatars);

            //        //item.Level = miniProfile.Level;
            //    }
            //}
            //_SteamUsersSourceList.Refresh();
            #endregion
        }

        public void SteamId_Click(SteamUser user)
        {
            if (user.WantsOfflineMode)
            {
                UserModeChange(user, false);
            }
            ReStartSteamByUser(user);
        }

        public void OfflineModeButton_Click(SteamUser user)
        {
            if (user.WantsOfflineMode == false)
            {
                UserModeChange(user, true);
            }
            ReStartSteamByUser(user);
        }

        private void ReStartSteamByUser(SteamUser user)
        {
            steamService.SetCurrentUser(user.AccountName ?? string.Empty);
            steamService.TryKillSteamProcess();
            steamService.StartSteam(SteamSettings.SteamStratParameter.Value);
        }

        private void UserModeChange(SteamUser user, bool OfflineMode)
        {
            user.WantsOfflineMode = OfflineMode;
            steamService.UpdateLocalUserData(user);
            user.OriginVdfString = user.CurrentVdfString;
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
                _SteamUsersSourceList.Remove(user);
            }
        }

        public async void OpenUserProfileUrl(SteamUser user)
        {
            await Browser2.OpenAsync(user.ProfileUrl);
        }

        public void LoginNewSteamAccount()
        {
            var result = MessageBox.ShowAsync(AppResources.UserChange_LoginNewAccountTip, button: MessageBox.Button.OKCancel).ContinueWith(s =>
            {
                if (s.Result == MessageBox.Result.OK)
                {
                    steamService.SetCurrentUser("");
                    steamService.TryKillSteamProcess();
                    steamService.StartSteam(SteamSettings.SteamStratParameter.Value);
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
