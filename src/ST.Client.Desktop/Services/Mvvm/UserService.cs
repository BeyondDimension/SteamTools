using ReactiveUI;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Properties;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services
{
    public class UserService : ReactiveObject
    {
        #region static members
        public static UserService Current { get; } = new();
        #endregion

        readonly IUserManager userManager = DI.Get<IUserManager>();

        public async void ShowWindow(CustomWindow windowName)
        {
            var isDialog = true;
            if (windowName == CustomWindow.LoginOrRegister)
            {
                var cUser = await userManager.GetCurrentUserAsync();
                if (cUser.HasValue()) return;
                isDialog = false;
            }
            else if (windowName == CustomWindow.ChangeBindPhoneNumber)
            {
                var cUser = await userManager.GetCurrentUserAsync();
                if (!cUser.HasValue()) return;
                if (string.IsNullOrWhiteSpace(cUser!.PhoneNumber)) return;
            }
            var vmType = Type.GetType($"System.Application.UI.ViewModels.{windowName}WindowViewModel");
            if (vmType != null && typeof(WindowViewModel).IsAssignableFrom(vmType))
            {
                await IShowWindowService.Instance.ShowDialog(vmType, windowName, isDialog: isDialog);
            }
        }

        public async Task SignOutAsync(Func<Task<IApiResponse>>? apiCall = null, string? message = null)
        {
            if (User == null) return;
            if (apiCall != null)
            {
                var rsp = await apiCall();
                if (!rsp.IsSuccess)
                {
                    return;
                }
            }
            await SignOutUserManagerAsync();
            await RefreshUserAsync();
            if (message != null)
            {
                Toast.Show(message);
            }
        }

        public async void SignOut()
        {
            if (!IsAuthenticated) return;
            var r = await MessageBoxCompat.ShowAsync(AppResources.User_SignOutTip, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel);
            if (r == MessageBoxResultCompat.OK)
            {
                await SignOutAsync(ICloudServiceClient.Instance.Manage.SignOut);
            }
        }

        public async void DelAccount()
        {
            if (!IsAuthenticated) return;
            var msg = AppResources.Success_.Format(AppResources.DelAccount);
            await SignOutAsync(ICloudServiceClient.Instance.Manage.DeleteAccount, msg);
        }

        public async Task SignOutUserManagerAsync()
        {
            await userManager.SignOutAsync();
        }

        UserInfoDTO? _User;
        public UserInfoDTO? User
        {
            get => _User;
            set => this.RaiseAndSetIfChanged(ref _User, value);
        }

        /// <summary>
        /// 指示当前用户是否已通过身份验证（已登录）
        /// </summary>
        public bool IsAuthenticated => User != null;

        SteamUser? _SteamUser;
        public SteamUser? CurrentSteamUser
        {
            get => _SteamUser;
            set => this.RaiseAndSetIfChanged(ref _SteamUser, value);
        }

        object GetAvaterPath(UserInfoDTO? user)
        {
            object? value = null;
            if (user is UserInfoDTO userInfo && userInfo.SteamAccountId.HasValue)
            {
                // Steam Avatar
                value = CurrentSteamUser?.AvatarStream;
            }

            if (user is IUserDTO user2 && user2.Avatar.HasValue)
            {
                // Guid Avatar
                value = ImageUrlHelper.GetImageApiUrlById(user2.Avatar.Value);
            }
            return value ?? DefaultAvaterPath;
        }

        const string DefaultAvaterPath = "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/AppResources/avater_default.png";

        object? _AvaterPath = DefaultAvaterPath;

        public object? AvaterPath
        {
            get => _AvaterPath;
            set => this.RaiseAndSetIfChanged(ref _AvaterPath, value);
        }

        public UserService()
        {
            this.WhenAnyValue(x => x.User)
                  .Subscribe(_ => this.RaisePropertyChanged(nameof(AvaterPath)));

            userManager.OnSignOut += () =>
            {
                User = null;
                CurrentSteamUser = null;
                AvaterPath = DefaultAvaterPath;
            };

            //ShowWindow = ReactiveCommand.Create<CustomWindow>(n => ShowWindowF(n));

            Task.Run(Initialize).ForgetAndDispose();
        }

        //public ICommand ShowWindow { get; }

        async void Initialize()
        {
            await RefreshUserAsync();
        }

        bool _HasPhoneNumber;
        public bool HasPhoneNumber
        {
            get => _HasPhoneNumber;
            set => this.RaiseAndSetIfChanged(ref _HasPhoneNumber, value);
        }

        public async Task RefreshUserAsync()
        {
            User = await userManager.GetCurrentUserInfoAsync();
            this.RaisePropertyChanged(nameof(IsAuthenticated));

            if (User != null && User.SteamAccountId.HasValue)
            {
                CurrentSteamUser = await ISteamworksWebApiService.Instance.GetUserInfo(User.SteamAccountId.Value);
                CurrentSteamUser.AvatarStream = IHttpService.Instance.GetImageAsync(CurrentSteamUser.AvatarFull, ImageChannelType.SteamAvatars);
                AvaterPath = CircleImageStream.Convert(await CurrentSteamUser.AvatarStream);
            }
            else
            {
                AvaterPath = DefaultAvaterPath;
            }

            var userInfo = await userManager.GetCurrentUserAsync();
            HasPhoneNumber = !string.IsNullOrWhiteSpace(userInfo?.PhoneNumber);
        }

        /// <summary>
        /// 更新当前登录用户的手机号码
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public async Task UpdateCurrentUserPhoneNumberAsync(string phoneNumber)
        {
            var user = await userManager.GetCurrentUserAsync();
            if (user == null) return;
            user.PhoneNumber = phoneNumber;
            await userManager.SetCurrentUserAsync(user);
            HasPhoneNumber = true;
        }

        /// <summary>
        /// 解绑账号后更新
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task UnbundleAccountAfterUpdateAsync(FastLoginChannel channel)
        {
            var user = await userManager.GetCurrentUserInfoAsync();
            if (user == null) return;
            switch (channel)
            {
                case FastLoginChannel.Steam:
                    user.SteamAccountId = null;
                    break;
                case FastLoginChannel.Microsoft:
                    user.MicrosoftAccountEmail = null;
                    break;
                case FastLoginChannel.QQ:
                    user.QQAccountNumber = null;
                    break;
                case FastLoginChannel.Apple:
                    user.AppleAccountEmail = null;
                    break;
                default:
                    return;
            }
            await userManager.SetCurrentUserInfoAsync(user, true);
            User = user;
        }
    }
}