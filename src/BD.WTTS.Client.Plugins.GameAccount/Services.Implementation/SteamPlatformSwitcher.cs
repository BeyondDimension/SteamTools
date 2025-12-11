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

    public async ValueTask<bool> SwapToAccount(IAccount? account, PlatformAccount platform)
    {
        await KillPlatformProcess(platform);
        var users = platform.Accounts?.Cast<SteamAccount>().Select(s => s.SteamUser).ToArray();
        if (users.Any_Nullable())
        {
            if (account is SteamAccount steamAccount && !string.IsNullOrEmpty(steamAccount?.AccountName))
            {
                await steamService.SetSteamCurrentUserAsync(steamAccount.AccountName);
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

                    var optional = SteamConnectService.Current.SteamUsers.Lookup(user.SteamId64);
                    if (optional.HasValue)
                    {
                        optional.Value.MostRecent = user.MostRecent;
                    }
                }
            }
            else
            {
                await ClearCurrentLoginUser(platform);
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
        return true;
    }

    public async ValueTask<bool> ClearCurrentLoginUser(PlatformAccount platform)
    {
        await steamService.SetSteamCurrentUserAsync(string.Empty);
        return true;
    }

    public async ValueTask<bool> KillPlatformProcess(PlatformAccount platform)
    {
        var r = await steamService.TryKillSteamProcess();
        return r;
    }

    public bool RunPlatformProcess(PlatformAccount platform, bool isAdmin)
    {
        steamService.StartSteamWithParameter();
        return true;
    }

    public async ValueTask NewUserLogin(PlatformAccount platform)
    {
        await ClearCurrentLoginUser(platform);
        await SwapToAccount(null, platform);
    }

    public ValueTask<bool> CurrnetUserAdd(string name, PlatformAccount platform) => ValueTask.FromResult(false);

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
        SteamConnectService.Current.RefreshSteamUsers();
        Task2.InBackground(async () =>
        {
            await SteamConnectService.Current.RefreshSteamUsersInfo();
            refreshUsers?.Invoke();
        });

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

        await Task.CompletedTask;
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
