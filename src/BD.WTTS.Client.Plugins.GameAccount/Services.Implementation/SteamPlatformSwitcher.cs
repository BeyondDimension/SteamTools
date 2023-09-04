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

    public void SwapToAccount(IAccount? account, PlatformAccount platform)
    {
        KillPlatformProcess(platform);
        var users = platform.Accounts?.Cast<SteamAccount>().Select(s => s.SteamUser);
        if (users.Any_Nullable())
        {
            if (account is SteamAccount steamAccount && !string.IsNullOrEmpty(steamAccount?.AccountName))
            {
                steamService.SetCurrentUser(steamAccount.AccountName);
                foreach (var user in users)
                {
                    if (user.AccountName == steamAccount.AccountName)
                    {
                        user.MostRecent = true;
                        user.RememberPassword = true;
                        user.WantsOfflineMode = steamAccount.WantsOfflineMode;
                        user.SkipOfflineModeWarning = steamAccount.SkipOfflineModeWarning;

                        if (steamAccount.PersonaState != PersonaState.Default)
                            ISteamService.Instance.SetPersonaState(steamAccount.SteamUser.SteamId32.ToString(), steamAccount.PersonaState);
                    }
                    else
                    {
                        user.MostRecent = false;
                    }
                }
            }
            else
            {
                steamService.SetCurrentUser("");
                foreach (var user in users)
                {
                    user.MostRecent = false;
                }
            }
            steamService.UpdateLocalUserData(users);
        }
        else
        {
            Toast.Show(ToastIcon.Error, Strings.Error_SteamGetUserInfo);
        }
        RunPlatformProcess(platform, false);
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
        SwapToAccount(null, platform);
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

    public async Task<IEnumerable<IAccount>?> GetUsers(PlatformAccount platform, Action? refreshUsers = null)
    {
        //var users = steamService.GetRememberUserList();
        await SteamConnectService.Current.RefreshSteamUsers();
        var users = SteamConnectService.Current.SteamUsers.Items;
        if (!users.Any_Nullable())
        {
            return null;
        }

        //        #region 加载备注信息和 JumpList

        //        var accountRemarks = Ioc.Get<IPartialGameAccountSettings>()?.AccountRemarks;

        //#if WINDOWS
        //        List<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)>? jumplistData = new();
        //#endif
        //        foreach (var user in users)
        //        {
        //            if (accountRemarks?.TryGetValue("Steam-" + user.SteamId64, out var remark) == true &&
        //                !string.IsNullOrEmpty(remark))
        //                user.Remark = remark;

        //#if WINDOWS
        //            {
        //                var title = user.SteamNickName ?? user.SteamId64.ToString(CultureInfo.InvariantCulture);
        //                if (!string.IsNullOrEmpty(user.Remark))
        //                {
        //                    title = user.SteamNickName + "(" + user.Remark + ")";
        //                }

        //                var processPath = Environment.ProcessPath;
        //                processPath.ThrowIsNull();
        //                if (!string.IsNullOrEmpty(user.AccountName)) jumplistData!.Add((title,
        //                    applicationPath: processPath,
        //                    iconResourcePath: processPath,
        //                    arguments: $"-clt steam -account {user.AccountName}",
        //                    description: Strings.UserChange_BtnTootlip,
        //                    customCategory: Strings.UserFastChange));
        //            }
        //#endif
        //        }

        //#if WINDOWS
        //        if (jumplistData.Any())
        //        {
        //            MainThread2.BeginInvokeOnMainThread(async () =>
        //            {
        //                var s = IJumpListService.Instance;
        //                await s.AddJumpItemsAsync(jumplistData);
        //            });
        //        }
        //#endif
        //        #endregion

        //        #region 通过webapi加载头像图片用户信息
        //        Task2.InBackground(async () =>
        //        {
        //            foreach (var user in users)
        //            {
        //                var temp = await swWebService.GetUserInfo(user.SteamId64);
        //                if (!string.IsNullOrEmpty(temp.SteamID))
        //                {
        //                    user.SteamID = temp.SteamID;
        //                    user.OnlineState = temp.OnlineState;
        //                    user.MemberSince = temp.MemberSince;
        //                    user.VacBanned = temp.VacBanned;
        //                    user.Summary = temp.Summary;
        //                    user.PrivacyState = temp.PrivacyState;
        //                    user.AvatarIcon = temp.AvatarIcon;
        //                    user.AvatarMedium = temp.AvatarMedium;
        //                    user.AvatarFull = temp.AvatarFull;
        //                    user.MiniProfile = temp.MiniProfile;
        //                    user.Level = user.MiniProfile?.Level;

        //                    //if (user.MiniProfile != null && !string.IsNullOrEmpty(user.MiniProfile.AnimatedAvatar))
        //                    //{
        //                    //    user.AvatarStream = imageHttpClientService.GetImageMemoryStreamAsync(user.MiniProfile.AnimatedAvatar, cache: true);
        //                    //}
        //                    //else
        //                    //{
        //                    //    user.AvatarStream = imageHttpClientService.GetImageMemoryStreamAsync(temp.AvatarFull, cache: true);
        //                    //}
        //                }
        //            }

        //            refreshUsers?.Invoke();
        //        });
        //        #endregion

        //        #region 加载动态头像头像框数据
        //        //foreach (var user in users)
        //        //{
        //        //    if (user.MiniProfile == null)
        //        //    {
        //        //        user.MiniProfile = await swWebService.GetUserMiniProfile(user.SteamId32);
        //        //    }
        //        //    if (user.MiniProfile != null)
        //        //    {
        //        //        if (!string.IsNullOrEmpty(user.MiniProfile.AnimatedAvatar))
        //        //            user.AvatarStream = imageHttpClientService.GetImageMemoryStreamAsync(user.MiniProfile.AnimatedAvatar, cache: true);

        //        //        if (!string.IsNullOrEmpty(user.MiniProfile.AvatarFrame))
        //        //            user.MiniProfile.AvatarFrameStream = imageHttpClientService.GetImageMemoryStreamAsync(user.MiniProfile.AvatarFrame, cache: true);

        //        //        user.Level = user.MiniProfile.Level;
        //        //    }
        //        //}
        //        #endregion

        return users.Select(s => new SteamAccount(s));
    }

    public bool SetPlatformPath(PlatformAccount platform)
    {
        return false;
    }

    public async Task<bool> DeleteAccountInfo(IAccount account, PlatformAccount platform)
    {
        if (account is SteamAccount steamAccount)
        {
            var result = await MessageBox.ShowAsync(Strings.UserChange_DeleteUserTip, button: MessageBox.Button.OKCancel);
            if (result == MessageBox.Result.OK)
            {
                result = await MessageBox.ShowAsync(Strings.UserChange_DeleteUserDataTip, button: MessageBox.Button.OKCancel);
                if (result == MessageBox.Result.OK)
                {
                    steamService.DeleteLocalUserData(steamAccount.SteamUser, true);
                }
                else
                {
                    steamService.DeleteLocalUserData(steamAccount.SteamUser, false);
                }

                platform.Accounts?.Remove(account);
                return true;
            }
        }
        return false;
    }
}
