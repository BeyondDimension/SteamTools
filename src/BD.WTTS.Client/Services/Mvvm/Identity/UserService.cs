using BD.WTTS.Models;
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public sealed class UserService : ReactiveObject
{
    static UserService? mCurrent;

    public static UserService Current => mCurrent ?? new();

    readonly IUserManager userManager = IUserManager.Instance;
    readonly IMicroServiceClient csc = IMicroServiceClient.Instance;
    readonly ISteamworksWebApiService steamworksWebApiService = ISteamworksWebApiService.Instance;
    readonly IWindowManager windowManager = IWindowManager.Instance;

    [Reactive]
    public IdentityUserInfoDTO? User { get; set; }

    /// <summary>
    /// 指示当前用户是否已通过身份验证（已登录）
    /// </summary>
    public bool IsAuthenticated => User != null;

    [Reactive]
    public SteamUser? CurrentSteamUser { get; set; }

    [Reactive]
    public object? AvatarPath { get; set; }

    /// <summary>
    /// 当前登录用户是否有手机号码
    /// </summary>
    [Reactive]
    public bool HasPhoneNumber { get; set; }

    /// <summary>
    /// 用于 UI 显示的当前登录用户的手机号码(隐藏中间四位)
    /// </summary>
    [Reactive]
    public string PhoneNumber { get; set; } = string.Empty;

    public async void ShowWindow()
    {
        var cUser = await userManager.GetCurrentUserAsync();
        if (cUser.HasValue()) return;

        //if (Random2.Next(2) == 1)
        //{
        var vm = new LoginOrRegisterWindowViewModel();
        await windowManager.ShowTaskDialogAsync(vm, vm.Title, isOkButton: false);
        //}
        //else
        //{
        //    INavigationService.Instance.Navigate(typeof(LoginOrRegisterWindowViewModel), NavigationTransitionEffect.DrillIn);
        //}
    }

    public void NavigateUserCenterPage()
    {
        // TODO: UserCenterPage
        throw new NotImplementedException();
        //if (IViewModelManager.Instance.MainWindow is MainWindowViewModel main)
        //{
        //    main.SelectedItem = new AccountPageViewModel();
        //}
    }

    public async Task SignOutAsync(Func<Task<IApiRsp>>? apiCall = null, string? message = null)
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
            Toast.Show(ToastIcon.Error, message);
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

    public DateTimeOffset? LastSignInTime { get; set; }

    public async Task SignInAsync()
    {
        if (User == null)
        {
            return;
        }
        if (User.IsSignIn == false)
        {
            var state = await csc.Manage.ClockIn();
            if (state.IsSuccess)
            {
                LastSignInTime = DateTimeOffset.Now;
                User.Experience = state.Content!.Experience;
                User.NextExperience = state.Content!.NextExperience;
                User.Level = state.Content!.Level;
                //User.EngineOil = state.Content!.Strength;
                User.IsSignIn = true;
                Toast.Show(ToastIcon.Success, AppResources.User_SignIn_Ok);
            }
            else
            {
                Toast.Show(ToastIcon.Error, state.Message);
            }
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

    private UserService()
    {
        mCurrent = this;

        this.WhenAnyValue(x => x.User)
              .Subscribe(_ => this.RaisePropertyChanged(nameof(AvatarPath)));

        userManager.OnSignOut += () =>
        {
            User = null;
            CurrentSteamUser = null;
            AvatarPath = null;
        };
        OpenAuthWattGameCommand = ReactiveCommand.Create<string>(url => OpenAuthUrl(url, FastLoginWebChannel.WattGame));
        OpenAuthOfficialCommand = ReactiveCommand.Create<string>(url => OpenAuthUrl(url, FastLoginWebChannel.OfficialWebsite));
        Task.Run(Initialize).ForgetAndDispose();
    }

    async void Initialize()
    {
        await RefreshUserAsync();
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

    public async Task SaveUserAsync(IdentityUserInfoDTO user)
    {
        await userManager.SetCurrentUserInfoAsync(user, true);

        await RefreshUserAsync(user, refreshCurrentUser: false);
    }

    public async Task RefreshUserAsync(IdentityUserInfoDTO? user, bool refreshCurrentUser = true)
    {
        User = user;
        this.RaisePropertyChanged(nameof(IsAuthenticated));

        if (refreshCurrentUser)
        {
            var currentUser = await userManager.GetCurrentUserAsync();
            RefreshCurrentUser(currentUser);
        }

        await RefreshUserAvatarAsync();
    }

    public async Task RefreshUserAsync(bool refreshCurrentUser = true)
    {
        var user = await userManager.GetCurrentUserInfoAsync();
        await RefreshUserAsync(user, refreshCurrentUser);
    }

    public async Task RefreshUserAvatarAsync()
    {
        if (User != null)
        {
            if (User.AvatarUrl.Any_Nullable())
            {
                var settingPriority = ExternalLoginChannel.Steam; // 设置中优先选取头像渠道配置项
                var order = new[] { settingPriority }.Concat(new[]
                {
                    ExternalLoginChannel.Steam,
                    ExternalLoginChannel.QQ,
                    ExternalLoginChannel.Apple,
                    ExternalLoginChannel.Microsoft,
                }.Where(x => x != settingPriority));
                foreach (var item in order)
                {
                    if (User.AvatarUrl!.TryGetValue(item, out var value))
                    {
                        var avatarUrl = value;
                        if (!string.IsNullOrWhiteSpace(avatarUrl))
                        {
                            AvatarPath = await ImageSource.GetAsync(avatarUrl, isCircle: true, cache: true);
                        }
                        return;
                    }
                    else if (item == ExternalLoginChannel.Steam
                        && await RefreshSteamUserAvatarAsync())
                    {
                        return;
                    }
                }
            }
            else if (await RefreshSteamUserAvatarAsync())
            {
                return;
            }

            async Task<bool> RefreshSteamUserAvatarAsync()
            {
                if (User != null && User.SteamAccountId.HasValue)
                {
                    CurrentSteamUser = await steamworksWebApiService.GetUserInfo(User.SteamAccountId.Value);

                    var avatarUri = CurrentSteamUser?.AvatarFull;
                    if (avatarUri != null)
                    {
                        var avatarStream = await ImageSource.GetAsync(avatarUri, isCircle: false, cache: true);
                        //CurrentSteamUser.AvatarStream = avatarStream;
                        if (avatarStream?.Stream != null)
                        {
                            var avatarPath = avatarStream.Clone();
                            avatarPath.Circle = true;
                            AvatarPath = avatarPath;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        AvatarPath = null;
    }

    //public async Task LoginShop()
    //{
    //    var temp = await csc.Shop.GetShopUserTokenAsync();
    //}

    public ICommand OpenAuthWattGameCommand { get; }

    public ICommand OpenAuthOfficialCommand { get; }

    /// <summary>
    /// 带登录状态跳转 Url
    /// </summary>
    /// <param name="url">跳转地址</param>
    /// <param name="channel">Web 跳转渠道</param>
    public async void OpenAuthUrl(string url, FastLoginWebChannel channel = FastLoginWebChannel.OfficialWebsite)
    {
        switch (channel)
        {

            case FastLoginWebChannel.OfficialWebsite:
                var token = await userManager.GetAuthTokenAsync();
                if (token == null)
                {
                    Browser2.Open(url.ToString());
                }
                else
                {
                    Browser2.Open(string.Format(Constants.Urls.OfficialWebsite_Fast_Login_, token.AccessToken, HttpUtility.UrlEncode(token.ExpiresIn.ToString()), HttpUtility.UrlEncode(url)));
                }
                break;
            case FastLoginWebChannel.WattGame:
                var shopToken = await userManager.GetShopAuthTokenAsync();
                if (shopToken == null)
                {
                    Browser2.Open(url.ToString());
                }
                else
                {
                    var cookieMaxAge = (shopToken.ExpiresIn - DateTimeOffset.Now).TotalSeconds;
                    Browser2.Open(string.Format(Constants.Urls.WattGame_Fast_Login_, shopToken.AccessToken, HttpUtility.UrlEncode(cookieMaxAge.ToString()), HttpUtility.UrlEncode(url)));
                }
                break;
            default:
                Browser2.Open(url.ToString());
                break;
        }
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
    public async Task UnbundleAccountAfterUpdateAsync(ExternalLoginChannel channel)
    {
        var user = await userManager.GetCurrentUserInfoAsync();
        if (user == null) return;
        switch (channel)
        {
            case ExternalLoginChannel.Steam:
                user.SteamAccountId = null;
                break;
            case ExternalLoginChannel.Microsoft:
                user.MicrosoftAccountEmail = null;
                break;
            case ExternalLoginChannel.QQ:
                user.QQNickName = null;
                break;
            case ExternalLoginChannel.Apple:
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
    public async Task BindAccountAfterUpdateAsync(ExternalLoginChannel channel, LoginOrRegisterResponse rsp)
    {
        var user = await userManager.GetCurrentUserInfoAsync();
        if (user == null) return;
        switch (channel)
        {
            case ExternalLoginChannel.Steam:
                user.SteamAccountId = rsp.User?.SteamAccountId;
                break;
            case ExternalLoginChannel.Microsoft:
                user.MicrosoftAccountEmail = rsp.User?.MicrosoftAccountEmail;
                break;
            case ExternalLoginChannel.QQ:
                user.QQNickName = rsp.User?.QQNickName;
                if (string.IsNullOrEmpty(user.NickName)) user.NickName = user.QQNickName ?? "";
                break;
            case ExternalLoginChannel.Apple:
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