using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using static System.Application.Services.CloudService.Constants;

namespace System.Application.UI.ViewModels
{
    public class SteamAccountPageViewModel : TabItemViewModel
    {
        public override TabItemId Id => TabItemId.SteamAccount;
        public override bool IsTaskBarSubMenu => true;

        readonly ISteamService steamService = DI.Get<ISteamService>();
        readonly IHttpService httpService = DI.Get<IHttpService>();
        readonly ISteamworksWebApiService webApiService = DI.Get<ISteamworksWebApiService>();

        public override string Name
        {
            get => AppResources.UserFastChange;
            protected set { throw new NotImplementedException(); }
        }

        public SteamAccountPageViewModel()
        {
            IconKey = nameof(SteamAccountPageViewModel).Replace("ViewModel", "Svg");

            this.WhenAnyValue(x => x.SteamUsers)
                  .Subscribe(s => this.RaisePropertyChanged(nameof(IsUserEmpty)));
            LoginAccountCommand = ReactiveCommand.Create(LoginNewSteamAccount);
            RefreshCommand = ReactiveCommand.Create(Initialize);

            ShareManageCommand = ReactiveCommand.Create(OpenShareManageWindow);
            MenuItems = new ObservableCollection<MenuItemViewModel>()
            {
                new MenuItemViewModel (nameof(AppResources.Refresh))
                            { IconKey="RefreshDrawing" , Command = RefreshCommand},
                new MenuItemViewModel (),
                new MenuItemViewModel(nameof(AppResources.UserChange_LoginNewAccount))
                            { IconKey="SteamDrawing", Command=LoginAccountCommand },
                new MenuItemViewModel(nameof(AppResources.AccountChange_Title)){
                      IconKey ="ChannelShareDrawing", Command = ShareManageCommand },
                //new MenuItemViewModel(nameof(AppResources.UserChange_RemarkReplaceName)){
                //      IconKey ="EditDrawing", Command = ShareManageCommand },
            };

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
        public void OpenShareManageWindow()
        {
            IShowWindowService.Instance.Show(CustomWindow.ShareManage, new ShareManageViewModel(), string.Empty, ResizeModeCompat.CanResize);
        }

        /// <summary>
        /// steam记住的用户列表
        /// </summary>
        private ReadOnlyObservableCollection<SteamUser>? _SteamUsers;
        private SourceCache<SteamUser, long> _SteamUsersSourceList;
        public ReadOnlyObservableCollection<SteamUser>? SteamUsers => _SteamUsers;

        public bool IsUserEmpty => !SteamUsers.Any_Nullable();

        public async override void Initialize()
        {
            var list = steamService.GetRememberUserList();

            if (!list.Any_Nullable())
            {
                //Toast.Show("");
                return;
            }

            #region 加载备注信息
            _SteamUsersSourceList.AddOrUpdate(list);
            var accountRemarks = Serializable.Clone<IReadOnlyDictionary<long, string?>?>(SteamAccountSettings.AccountRemarks.Value);

            foreach (var user in _SteamUsersSourceList.Items)
            {
                string? remark = null;
                accountRemarks?.TryGetValue(user.SteamId64, out remark);
                user.Remark = remark;
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
                    user.AvatarStream = httpService.GetImageAsync(temp.AvatarFull, ImageChannelType.SteamAvatars);
                }
                if (OperatingSystem2.IsWindows)
                {
                    var title = user.SteamNickName ?? user.SteamId64.ToString(CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(user.Remark))
                    {
                        title = user.SteamNickName + "(" + user.Remark + ")";
                    }

                    DI.Get<ISystemJumpListService>().AddJumpTask(title, AppHelper.ProgramPath, AppHelper.ProgramPath, "-clt steam -account " + user.AccountName, AppResources.UserChange_BtnTootlip, this.Name);
                }
            }

            _SteamUsersSourceList.Refresh();
            #endregion

            #region 加载动态头像头像框数据
            foreach (var item in _SteamUsersSourceList.Items)
            {
                item.MiniProfile = await webApiService.GetUserMiniProfile(item.SteamId3_Int);
                var miniProfile = item.MiniProfile;
                if (miniProfile != null)
                {
                    if (!string.IsNullOrEmpty(miniProfile.AnimatedAvatar))
                        item.AvatarStream = httpService.GetImageAsync(miniProfile.AnimatedAvatar, ImageChannelType.SteamAvatars);
                    //miniProfile.AvatarFrameStream = httpService.GetImageAsync(miniProfile.AvatarFrame, ImageChannelType.SteamAvatars);
                }
            }

            _SteamUsersSourceList.Refresh();
            #endregion

            //this.WhenAnyValue(x => x.SteamUsers)
            //      .Subscribe(items => items?
            //      .ToObservableChangeSet()
            //      .AutoRefresh(x => x.Remark)
            //      .WhenValueChanged(x => x.Remark, false)
            //      .Subscribe(_ =>
            //      {
            //          SteamAccountSettings.AccountRemarks.Value = items?.Where(x => !string.IsNullOrEmpty(x.Remark)).ToDictionary(k => k.SteamId64, v => v.Remark);
            //      }));
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
            var result = await MessageBoxCompat.ShowAsync(@AppResources.UserChange_DeleteUserTip, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel);
            if (result == MessageBoxResultCompat.OK)
            {
                result = await MessageBoxCompat.ShowAsync(@AppResources.UserChange_DeleteUserDataTip, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel);
                if (result == MessageBoxResultCompat.OK)
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

        public void OpenUserProfileUrl(SteamUser user)
        {
            BrowserOpen(user.ProfileUrl);
        }

        public void LoginNewSteamAccount()
        {
            var result = MessageBoxCompat.ShowAsync(@AppResources.UserChange_LoginNewAccountTip, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(s =>
            {
                if (s.Result == MessageBoxResultCompat.OK)
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

            SteamAccountSettings.AccountRemarks.Value!.AddOrUpdate(user.SteamId64, user.Remark, (oldkey, oldvalue) => user.Remark);
            SteamAccountSettings.AccountRemarks.RaiseValueChanged();
        }
    }
}