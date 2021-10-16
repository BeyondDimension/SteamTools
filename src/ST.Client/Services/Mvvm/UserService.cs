using ReactiveUI;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Properties;
using System.Linq;
using System.Threading.Tasks;
using System.Application.UI;

// ReSharper disable once CheckNamespace
namespace System.Application.Services
{
    public sealed class UserService : MvvmService<UserService>
    {
        readonly IUserManager userManager = IUserManager.Instance;
        readonly ICloudServiceClient csc = ICloudServiceClient.Instance;
        readonly IHttpService httpService = IHttpService.Instance;
        readonly ISteamworksWebApiService steamworksWebApiService = ISteamworksWebApiService.Instance;
        readonly IWindowManager windowManager = IWindowManager.Instance;

        public async void ShowWindow(CustomWindow windowName)
        {
            var isDialog = true;
            if (windowName == CustomWindow.LoginOrRegister)
            {
                var cUser = await userManager.GetCurrentUserAsync();
                if (cUser.HasValue()) return;
                isDialog = false;
            }
            else if (windowName == CustomWindow.UserProfile)
            {
                isDialog = false;
            }
            else if (windowName == CustomWindow.ChangeBindPhoneNumber)
            {
                var cUser = await userManager.GetCurrentUserAsync();
                if (!cUser.HasValue()) return;
                if (string.IsNullOrWhiteSpace(cUser!.PhoneNumber)) return;
            }
            await windowManager.ShowDialog(windowName, isDialog: isDialog);
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
            var r = await MessageBox.ShowAsync(AppResources.User_SignOutTip, button: MessageBox.Button.OKCancel);
            if (r == MessageBox.Result.OK)
            {
                await SignOutAsync(csc.Manage.SignOut);
            }
        }

        public async Task DelAccountAsync()
        {
            if (!IsAuthenticated) return;
            var msg = AppResources.Success_.Format(AppResources.DelAccount);
            await SignOutAsync(csc.Manage.DeleteAccount, msg);
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

        public string DefaultAvaterPath
        {
            get
            {
                if (IApplication.Type == IApplication.AppType.Avalonia)
                {
                    return "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/AppResources/avater_default.png";
                }
                return string.Empty;
            }
        }

        object? _AvaterPath;

        public object? AvaterPath
        {
            get => _AvaterPath ?? DefaultAvaterPath;
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

            Task.Run(Initialize).ForgetAndDispose();
        }

        async void Initialize()
        {
            await RefreshUserAsync();
        }

        bool _HasPhoneNumber;
        /// <summary>
        /// 当前登录用户是否有手机号码
        /// </summary>
        public bool HasPhoneNumber
        {
            get => _HasPhoneNumber;
            set => this.RaiseAndSetIfChanged(ref _HasPhoneNumber, value);
        }

        string _PhoneNumber = string.Empty;
        /// <summary>
        /// 用于 UI 显示的当前登录用户的手机号码(隐藏中间四位)
        /// </summary>
        public string PhoneNumber
        {
            get => _PhoneNumber;
            set => this.RaiseAndSetIfChanged(ref _PhoneNumber, value);
        }

        static string GetCurrentUserPhoneNumber(CurrentUser? user, bool notHideMiddleFour = false)
        {
            var phone_number = user?.PhoneNumber;
            if (string.IsNullOrWhiteSpace(phone_number)) return AppResources.Unbound;
            return notHideMiddleFour ? phone_number : PhoneNumberHelper.ToStringHideMiddleFour(phone_number);
        }

        public void RefreshCurrentUser(CurrentUser? currentUser)
        {
            PhoneNumber = GetCurrentUserPhoneNumber(currentUser);
            HasPhoneNumber = !string.IsNullOrWhiteSpace(currentUser?.PhoneNumber);
        }

        public async Task RefreshUserAsync(UserInfoDTO? user)
        {
            User = user;
            this.RaisePropertyChanged(nameof(IsAuthenticated));

            await RefreshUserAvaterAsync();

            var currentUser = await userManager.GetCurrentUserAsync();
            RefreshCurrentUser(currentUser);
        }

        public async Task RefreshUserAsync()
        {
            var user = await userManager.GetCurrentUserInfoAsync();
            await RefreshUserAsync(user);
        }

        public async Task RefreshUserAvaterAsync()
        {
            if (User != null)
            {
                if (User.AvatarUrl.Any_Nullable())
                {
                    var settingPriority = FastLoginChannel.Steam; // 设置中优先选取头像渠道配置项
                    var order = new[] { settingPriority }.Concat(new[] {
                        FastLoginChannel.Steam,
                        FastLoginChannel.QQ,
                        FastLoginChannel.Apple,
                        FastLoginChannel.Microsoft,
                    }.Where(x => x != settingPriority));
                    foreach (var item in order)
                    {
                        if (User.AvatarUrl!.ContainsKey(item))
                        {
                            var avatarUrl = User.AvatarUrl[item];
                            if (!string.IsNullOrWhiteSpace(avatarUrl))
                            {
                                var avatarLocalFilePath = await httpService.GetImageAsync(avatarUrl, ImageChannelType.SteamAvatars);
                                var avaterSouce = ImageSouce.TryParse(avatarLocalFilePath, isCircle: true);
                                AvaterPath = avaterSouce ?? DefaultAvaterPath;
                            }
                            return;
                        }
                        else if (item == FastLoginChannel.Steam
                            && await RefreshSteamUserAvaterAsync())
                        {
                            return;
                        }
                    }
                }
                else if (await RefreshSteamUserAvaterAsync())
                {
                    return;
                }

                async Task<bool> RefreshSteamUserAvaterAsync()
                {
                    if (User != null && User.SteamAccountId.HasValue)
                    {
                        CurrentSteamUser = await steamworksWebApiService.GetUserInfo(User.SteamAccountId.Value);
                        CurrentSteamUser.AvatarStream = httpService.GetImageAsync(CurrentSteamUser.AvatarFull, ImageChannelType.SteamAvatars);
                        var avaterSouce = ImageSouce.TryParse(await CurrentSteamUser.AvatarStream, isCircle: true);
                        AvaterPath = avaterSouce ?? DefaultAvaterPath;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            AvaterPath = DefaultAvaterPath;
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
            RefreshCurrentUser(user);
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
                    user.QQNickName = null;
                    break;
                case FastLoginChannel.Apple:
                    user.AppleAccountEmail = null;
                    break;
                default:
                    return;
            }
            if (user.AvatarUrl != null && user.AvatarUrl.ContainsKey(channel))
            {
                user.AvatarUrl.Remove(channel);
            }
            await userManager.SetCurrentUserInfoAsync(user, true);
            await RefreshUserAsync(user);
        }

        /// <summary>
        /// 绑定账号后更新
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="rsp"></param>
        /// <returns></returns>
        public async Task BindAccountAfterUpdateAsync(FastLoginChannel channel, LoginOrRegisterResponse rsp)
        {
            var user = await userManager.GetCurrentUserInfoAsync();
            if (user == null) return;
            switch (channel)
            {
                case FastLoginChannel.Steam:
                    user.SteamAccountId = rsp.User?.SteamAccountId;
                    break;
                case FastLoginChannel.Microsoft:
                    user.MicrosoftAccountEmail = rsp.User?.MicrosoftAccountEmail;
                    break;
                case FastLoginChannel.QQ:
                    user.QQNickName = rsp.User?.QQNickName;
                    if (string.IsNullOrEmpty(user.NickName)) user.NickName = user.QQNickName ?? "";
                    break;
                case FastLoginChannel.Apple:
                    user.AppleAccountEmail = rsp.User?.AppleAccountEmail;
                    break;
                default:
                    return;
            }
            if (rsp.User != null)
            {
                if (!string.IsNullOrEmpty(rsp.User.NickName) && string.IsNullOrEmpty(user.NickName)) user.NickName = rsp.User.NickName;
                if (rsp.User.Gender != default && user.Gender != rsp.User.Gender) user.Gender = rsp.User.Gender;
                if (rsp.User.AvatarUrl != null && rsp.User.AvatarUrl.ContainsKey(channel))
                {
                    if (user.AvatarUrl == null)
                    {
                        user.AvatarUrl = new()
                        {
                            { channel, rsp.User.AvatarUrl[channel] }
                        };
                    }
                    else if (user.AvatarUrl.ContainsKey(channel))
                    {
                        user.AvatarUrl[channel] = rsp.User.AvatarUrl[channel];
                    }
                    else
                    {
                        user.AvatarUrl.Add(channel, rsp.User.AvatarUrl[channel]);
                    }
                }
            }
            await userManager.SetCurrentUserInfoAsync(user, true);
            await RefreshUserAsync(user);
        }
    }
}