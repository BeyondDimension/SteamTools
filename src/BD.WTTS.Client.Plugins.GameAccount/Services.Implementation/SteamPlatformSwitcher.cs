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

    public void SwapToUser(IAccount account)
    {
        if (!string.IsNullOrEmpty(account.AccountName))
        {
            KillPlatformProcess();
            steamService.SetCurrentUser(account.AccountName);
            //steamService.UpdateLocalUserData(SteamUsers!);
            RunPlatformProcess();
        }
    }

    public bool KillPlatformProcess()
    {
        return steamService.TryKillSteamProcess();
    }

    public void RunPlatformProcess()
    {
        steamService.StartSteamWithParameter();
    }

    public void NewUserLogin()
    {

        KillPlatformProcess();
        steamService.SetCurrentUser("");
        //steamService.UpdateLocalUserData(SteamUsers!);
        RunPlatformProcess();
    }

    public bool CurrnetUserAdd(string name) => false;

    public void ChangeUserRemark()
    {

    }

    public async Task<IEnumerable<IAccount>?> GetUsers()
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
}
