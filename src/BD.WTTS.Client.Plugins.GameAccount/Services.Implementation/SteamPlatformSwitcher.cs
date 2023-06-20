namespace BD.WTTS.Services.Implementation;

public sealed class SteamPlatformSwitcher : IPlatformSwitcher
{
    readonly ISteamService steamService;
    readonly ISteamworksWebApiService swWebService;

    public SteamPlatformSwitcher(ISteamService steamService, ISteamworksWebApiService swWebService)
    {
        this.steamService = steamService;
        this.swWebService = swWebService;
    }

    public void SwapToAccount(IAccount account, PlatformAccount platform)
    {
        if (!string.IsNullOrEmpty(account.AccountName))
        {
            KillPlatformProcess(platform);
            steamService.SetCurrentUser(account.AccountName);
            //steamService.UpdateLocalUserData(SteamUsers!);
            RunPlatformProcess(platform, false);
        }
    }

    bool IPlatformSwitcher.ClearCurrentLoginUser(PlatformAccount platform)
    {
        steamService.SetCurrentUser("");
        return true;
    }

    public bool KillPlatformProcess(PlatformAccount platform)
    {
        return steamService.TryKillSteamProcess();
    }

    public bool RunPlatformProcess(PlatformAccount platform, bool isAdmin)
    {
        steamService.StartSteamWithParameter();
        return true;
    }

    public void NewUserLogin(PlatformAccount platform)
    {
        KillPlatformProcess(platform);
        steamService.SetCurrentUser("");
        //steamService.UpdateLocalUserData(SteamUsers!);
        RunPlatformProcess(platform, false);
    }

    public bool CurrnetUserAdd(string name, PlatformAccount platform) => false;

    public string GetCurrentAccountId(PlatformAccount platform)
    {
        var user = steamService.GetRememberUserList().FirstOrDefault(s => s.MostRecent);
        if (user != null)
        {
            return user.SteamId64.ToString();
        }
        return "";
    }

    public void ChangeUserRemark()
    {

    }

    public async Task<IEnumerable<IAccount>?> GetUsers(PlatformAccount platform)
    {
        var users = steamService.GetRememberUserList();
        if (!users.Any_Nullable())
        {
            return null;
        }

        #region 通过webapi加载头像图片用户信息
        foreach (var user in users)
        {
            var temp = await swWebService.GetUserInfo(user.SteamId64);
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

                //if (user.MiniProfile != null && !string.IsNullOrEmpty(user.MiniProfile.AnimatedAvatar))
                //{
                //    user.AvatarStream = imageHttpClientService.GetImageMemoryStreamAsync(user.MiniProfile.AnimatedAvatar, cache: true);
                //}
                //else
                //{
                //    user.AvatarStream = imageHttpClientService.GetImageMemoryStreamAsync(temp.AvatarFull, cache: true);
                //}
            }
        }

        #endregion

        #region 加载动态头像头像框数据
        //foreach (var user in users)
        //{
        //    if (user.MiniProfile == null)
        //    {
        //        user.MiniProfile = await swWebService.GetUserMiniProfile(user.SteamId32);
        //    }
        //    if (user.MiniProfile != null)
        //    {
        //        if (!string.IsNullOrEmpty(user.MiniProfile.AnimatedAvatar))
        //            user.AvatarStream = imageHttpClientService.GetImageMemoryStreamAsync(user.MiniProfile.AnimatedAvatar, cache: true);

        //        if (!string.IsNullOrEmpty(user.MiniProfile.AvatarFrame))
        //            user.MiniProfile.AvatarFrameStream = imageHttpClientService.GetImageMemoryStreamAsync(user.MiniProfile.AvatarFrame, cache: true);

        //        user.Level = user.MiniProfile.Level;
        //    }
        //}
        #endregion

        return users.Select(s => new SteamAccount(s));
    }

    public bool SetPlatformPath(PlatformAccount platform)
    {

        return false;
    }
}
