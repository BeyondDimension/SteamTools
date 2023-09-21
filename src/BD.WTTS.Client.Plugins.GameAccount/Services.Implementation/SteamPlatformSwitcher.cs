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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async void SetCurrentUser(string userName = "")
    {
        await steamService.SetSteamCurrentUserAsync(userName);
    }

    public void SwapToAccount(IAccount? account, PlatformAccount platform)
    {
        KillPlatformProcess(platform);
        var users = platform.Accounts?.Cast<SteamAccount>().Select(s => s.SteamUser);
        if (users.Any_Nullable())
        {
            if (account is SteamAccount steamAccount && !string.IsNullOrEmpty(steamAccount?.AccountName))
            {
                SetCurrentUser(steamAccount.AccountName);
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
                SetCurrentUser();
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
        SetCurrentUser();
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

        var accounts = users.Select(s => new SteamAccount(s));

        var trayMenus = accounts.Select(user =>
        {
            var title = user.DisplayName ?? user.AccountId;
            if (!string.IsNullOrEmpty(user.AliasName))
            {
                title = user.DisplayName + "(" + user.AliasName + ")";
            }
            return new TrayMenuItem
            {
                Name = title,
                Command = platform.SwapToAccountCommand,
                CommandParameter = user,
            };
        }).ToList();

        IApplication.Instance.UpdateMenuItems(Plugin.Instance.UniqueEnglishName, new TrayMenuItem
        {
            Name = Plugin.Instance.Name,
            Items = trayMenus,
        });

        return accounts;
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
