using DynamicData.Binding;
using ReactiveUI;
using System.Application.Entities;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Linq;
using System.Linq.Expressions;
using System.Properties;
using System.Threading.Tasks;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class UserProfileWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.UserProfile;

        byte[]? userInfoValue;
        UserInfoDTO? userInfoSource;
        readonly IUserManager userManager;

        public UserProfileWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);

            userManager = DI.Get<IUserManager>();
            Initialize_Ctor();
            async void Initialize_Ctor()
            {
                this.InitAreas();
                userInfoSource = await userManager.GetCurrentUserInfoAsync() ?? new UserInfoDTO();
                userInfoValue = Serializable.SMP(userInfoSource);

                // (SetFields)DTO => VM
                _UID = userInfoSource.Id.ToString();
                _NickName = userInfoSource.NickName;
                _Gender = userInfoSource.Gender;
                _BirthDate = userInfoSource.GetBirthDate();
                if (userInfoSource.AreaId.HasValue) this.SetAreaId(userInfoSource.AreaId.Value);

                userInfoSource = Serializable.DMP<UserInfoDTO>(userInfoValue) ?? throw new ArgumentNullException(nameof(userInfoSource));
                // IsModifySubscribe
                void SubscribeOnNext<T>(T? value, Action<T?> onNext)
                {
                    onNext(value);
                    DetectionIsModify();
                }
                void SubscribeFormItem<T>(Expression<Func<UserProfileWindowViewModel, T?>> expression, Action<T?> onNext) => this.WhenValueChanged(expression, false).Subscribe(value => SubscribeOnNext(value, onNext)).AddTo(this);
                SubscribeFormItem(x => x.NickName, x => userInfoSource.NickName = x ?? string.Empty);
                SubscribeFormItem(x => x.Gender, x => userInfoSource.Gender = x);
                SubscribeFormItem(x => x.BirthDate, x => userInfoSource.SetBirthDate(x));
                void SubscribeAreaOnNext(IArea? area)
                {
                    var areaId = area?.Id;
                    userInfoSource.AreaId = (!areaId.HasValue ||
                        areaId.Value == AreaUIHelper.PleaseSelect.Id) ?
                        this.GetSelectAreaId() : areaId;
                }
                SubscribeFormItem(x => x.AreaSelectItem2, SubscribeAreaOnNext);
                SubscribeFormItem(x => x.AreaSelectItem3, SubscribeAreaOnNext);
                SubscribeFormItem(x => x.AreaSelectItem4, SubscribeAreaOnNext);
            }
            OnBindFastLoginClick = ReactiveCommand.CreateFromTask<string>(async channel_ =>
            {
                if (Enum.TryParse<FastLoginChannel>(channel_, out var channel))
                {
                    CurrentSelectChannel = channel_;
                    await ThirdPartyLoginHelper.StartAsync(this, channel, isBind: true);
                }
            });
            OnManualLoginClick = ThirdPartyLoginHelper.ManualLogin;
            OnUnbundleFastLoginClick = ReactiveCommand.CreateFromTask<string>(async channel_ =>
            {
                if (Enum.TryParse<FastLoginChannel>(channel_, out var channel))
                {
                    await OnUnbundleFastLoginClickAsync(channel);
                }
            });
            OnCancelBindFastLoginClick = ReactiveCommand.Create(HideFastLoginLoading);
            UIDCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await UIDCopyToClipboardAsync(_UID);
            });
        }

        string? _CurrentSelectChannel;
        public string? CurrentSelectChannel
        {
            get => _CurrentSelectChannel;
            set => this.RaiseAndSetIfChanged(ref _CurrentSelectChannel, value);
        }

        /// <summary>
        /// 隐藏快速登录加载中动画
        /// </summary>
        void HideFastLoginLoading() => CurrentSelectChannel = null;

        bool _IsModify;
        public bool IsModify
        {
            get => _IsModify;
            set => this.RaiseAndSetIfChanged(ref _IsModify, value);
        }

        string? _NickName;
        public string? NickName
        {
            get => _NickName;
            set => this.RaiseAndSetIfChanged(ref _NickName, value);
        }

        string? _UID;
        public string? UID
        {
            get => _UID;
            set => this.RaiseAndSetIfChanged(ref _UID, value);
        }

        Gender _Gender;
        public Gender Gender
        {
            get => _Gender;
            set => this.RaiseAndSetIfChanged(ref _Gender, value);
        }

        DateTimeOffset? _BirthDate;
        public DateTimeOffset? BirthDate
        {
            get => _BirthDate;
            set => this.RaiseAndSetIfChanged(ref _BirthDate, value);
        }

        public bool IsComplete { get; set; }

        public async void Submit()
        {
            if (!IsModify || IsLoading) return;

            if (userInfoSource == null) throw new ArgumentNullException(nameof(userInfoSource));

            IsLoading = true;

            var request = new EditUserProfileRequest
            {
                NickName = NickName ?? string.Empty,
                Avatar = userInfoSource.Avatar,
                Gender = Gender,
                BirthDate = userInfoSource.BirthDate,
                BirthDateTimeZone = userInfoSource.BirthDateTimeZone,
                AreaId = this.GetSelectAreaId(),
            };

            var response = await ICloudServiceClient.Instance.Manage.EditUserProfile(request);
            if (response.IsSuccess)
            {
                await userManager.SetCurrentUserInfoAsync(userInfoSource, true);
                await UserService.Current.RefreshUserAsync();

                IsComplete = true;
                var msg = AppResources.Success_.Format(AppResources.User_EditProfile);
                Toast.Show(msg);
                Close?.Invoke();
            }

            IsLoading = false;
        }

        public new Action? Close { get; set; }

        bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }

        /// <summary>
        /// 换绑手机号码 按钮点击
        /// </summary>
        public ICommand OnBtnChangeBindPhoneNumberClick { get; } = ReactiveCommand.Create(() =>
        {
            UserService.Current.ShowWindow(CustomWindow.ChangeBindPhoneNumber);
        });

        /// <summary>
        /// 绑定手机号码 按钮点击
        /// </summary>
        public ICommand OnBtnBindPhoneNumberClick { get; } = ReactiveCommand.Create(() =>
        {
            UserService.Current.ShowWindow(CustomWindow.BindPhoneNumber);
        });

        /// <summary>
        /// 绑定用于快速登录的第三方账号 按钮点击
        /// </summary>
        public ICommand OnBindFastLoginClick { get; }

        /// <summary>
        /// 手动输入数据
        /// </summary>
        public ICommand OnManualLoginClick { get; }

        public ICommand OnCancelBindFastLoginClick { get; }

        /// <summary>
        /// 解绑用于快速登录的第三方账号 按钮点击
        /// </summary>
        public ICommand OnUnbundleFastLoginClick { get; }

        public ICommand UIDCommand { get; }

        async Task OnUnbundleFastLoginClickAsync(FastLoginChannel channel)
        {
            if (!UserService.Current.HasPhoneNumber)
            {
                return;
            }

            if (IsLoading) return;

            var r = await MessageBox.ShowAsync(AppResources.User_UnbundleAccountTip, button: MessageBox.Button.OKCancel);
            if (r.IsOK())
            {
                IsLoading = true;

                var response = await ICloudServiceClient.Instance.Manage.UnbundleAccount(channel);

                if (response.IsSuccess)
                {
                    await UserService.Current.UnbundleAccountAfterUpdateAsync(channel);
                    var msg = AppResources.Success_.Format(AppResources.Unbundling);
                    Toast.Show(msg);
                }

                IsLoading = false;
            }
        }

        /// <summary>
        /// 检查是否有修改
        /// </summary>
        void DetectionIsModify()
        {
            var currentUserInfoValue = Serializable.SMP(userInfoSource);
            IsModify = userInfoValue == null || !Enumerable.SequenceEqual(userInfoValue, currentUserInfoValue);
        }

        /// <summary>
        /// 绑定成功时刷新用户资料
        /// </summary>
        void OnBindSuccessedRefreshUser()
        {
            var user = UserService.Current.User;
            if (user == null || Disposed || userInfoValue == null) return;
            var changeNickName = string.IsNullOrEmpty(NickName);
            if (changeNickName)
            {
                var user2 = Serializable.DMP<UserInfoDTO>(userInfoValue)!;
                user2.NickName = user.NickName;
                userInfoValue = Serializable.SMP(user2);
                NickName = user.NickName;
            }
        }

        public static async Task UIDCopyToClipboardAsync(string? uid)
        {
            if (!string.IsNullOrWhiteSpace(uid))
            {
                await Clipboard2.SetTextAsync(uid);
                Toast.Show(AppResources.CopyToClipboard);
            }
        }
    }
}
