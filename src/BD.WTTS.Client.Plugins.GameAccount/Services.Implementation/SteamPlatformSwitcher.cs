using Avalonia.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Services.Implementation;

public sealed class SteamPlatformSwitcher : IPlatformSwitcher
{
    readonly ISteamService steamService;
    readonly ConditionalWeakTable<SteamUser, SteamAccount> trayAccounts = new();
    int switching;

    public SteamPlatformSwitcher(ISteamService steamService)
    {
        this.steamService = steamService;
    }

    internal void RegisterTrayAccounts(PlatformAccount platform)
    {
        var steam = SteamConnectService.Current;
        var accounts = Observable.Create<TrayAccountsSnapshot>(observer =>
        {
            TrayAccountsSnapshot? previous = null;
            int queued = 0, disposed = 0;

            void QueueSnapshot()
            {
                if (Interlocked.Exchange(ref queued, 1) != 0)
                    return;

                // Batch cache clears/reloads and related property notifications on the UI thread.
                Dispatcher.UIThread.Post(() =>
                {
                    Interlocked.Exchange(ref queued, 0);
                    if (Volatile.Read(ref disposed) != 0)
                        return;
                    var status = platform.IsLoading ? TrayAccountsStatus.Loading :
                        platform.UsersLoadException != null ? TrayAccountsStatus.Failed :
                        platform.HasLoadedUsers ? TrayAccountsStatus.Loaded : TrayAccountsStatus.Loading;
                    var currentId = GetConfirmedAccountId(steam);
                    TrayAccountItem[] rows = status == TrayAccountsStatus.Loaded
                        ? steam.SteamUsers.Items
                            .OrderByDescending(user => user.SteamId64 == currentId)
                            .ThenByDescending(user => user.MostRecent)
                            .ThenByDescending(user => user.LastLoginTime)
                            .ThenBy(user => user.SteamId64)
                            .Select(user => CreateTrayRow(user, currentId, platform.SwapToAccountCommand))
                            .ToArray()
                        : [];
                    var snapshot = new TrayAccountsSnapshot(status, rows);

                    if (previous is not { } last || last.Status != status || !last.Rows.SequenceEqual(rows))
                    {
                        previous = snapshot;
                        observer.OnNext(snapshot);
                    }
                });
            }

            var cacheSubscription = steam.SteamUsers.Connect()
                .AutoRefreshOnObservable(user => ObserveProperties(user).Where(change => change.PropertyName is
                    null or "" or nameof(SteamUser.Remark) or nameof(SteamUser.MostRecent) or
                    nameof(SteamUser.AvatarMedium) or nameof(SteamUser.AvatarIcon) or nameof(SteamUser.AvatarFull)))
                .Subscribe(_ => QueueSnapshot());
            var connectionSubscription = ObserveProperties(steam)
                .Where(change => change.PropertyName is nameof(SteamConnectService.CurrentSteamUser) or
                    nameof(SteamConnectService.IsConnectToSteam))
                .Subscribe(_ => QueueSnapshot());
            var loadSubscription = ObserveProperties(platform)
                .Where(static change => change.PropertyName == nameof(PlatformAccount.IsLoading))
                .Subscribe(_ => QueueSnapshot());
            QueueSnapshot();
            if (!platform.IsLoading && !platform.HasLoadedUsers && platform.UsersLoadException == null)
                platform.LoadUsers();

            return Disposable.Create(() =>
            {
                Volatile.Write(ref disposed, 1);
                cacheSubscription.Dispose();
                connectionSubscription.Dispose();
                loadSubscription.Dispose();
            });
        });

        IApplication.Instance.UpdateMenuItems(Plugin.Instance.UniqueEnglishName, new TrayMenuItem
        {
            Name = Strings.UserFastChange,
            NameResourceKey = nameof(Strings.UserFastChange),
            Command = ReactiveCommand.Create(() =>
            {
                IApplication.Instance.RestoreMainWindow();
                INavigationService.Instance.Navigate(typeof(GameAccountPage));
            }),
            Accounts = accounts,
            RefreshAccountsCommand = ReactiveCommand.Create(platform.LoadUsers),
        });
    }

    static IObservable<PropertyChangedEventArgs> ObserveProperties(INotifyPropertyChanged source) =>
        Observable.Create<PropertyChangedEventArgs>(observer =>
        {
            PropertyChangedEventHandler handler = (_, args) => observer.OnNext(args);
            source.PropertyChanged += handler;
            return Disposable.Create(() => source.PropertyChanged -= handler);
        });

    static long? GetConfirmedAccountId(SteamConnectService steam) =>
        steam.IsConnectToSteam ? steam.CurrentSteamUser?.SteamId64 : null;

    TrayAccountItem CreateTrayRow(SteamUser user, long? currentId, ICommand command)
    {
        var nickname = DisplayText(user.SteamNickName, user.AccountName) ?? DisplayText(user.PersonaName, user.AccountName);
        var remark = DisplayText(user.Remark, user.AccountName);
        var avatar = StaticAvatarUrl(user.AvatarMedium) ?? StaticAvatarUrl(user.AvatarIcon) ?? StaticAvatarUrl(user.AvatarFull);
        return new TrayAccountItem(remark ?? nickname ?? $"{Strings.SteamAccount} ({user.SteamId64})",
            remark is not null && !string.Equals(remark, nickname, StringComparison.Ordinal) ? nickname : null,
            avatar, user.SteamId64 == currentId, user.MostRecent, command,
            trayAccounts.GetValue(user, static value => new SteamAccount(value)));
    }

    static string? DisplayText(string? text, string? login) =>
        string.IsNullOrWhiteSpace(text) || string.Equals(text.Trim(), login?.Trim(), StringComparison.OrdinalIgnoreCase)
            ? null : text.Trim();

    static string? StaticAvatarUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps && uri.UserInfo.Length == 0
            ? uri.AbsoluteUri : null;

    public async ValueTask<bool> SwapToAccount(IAccount? account, PlatformAccount platform)
    {
        if (Interlocked.CompareExchange(ref switching, 1, 0) != 0)
            return false;

        try
        {
            return await SwapToAccountCore(account, platform);
        }
        finally
        {
            Volatile.Write(ref switching, 0);
        }
    }

    async ValueTask<bool> SwapToAccountCore(IAccount? account, PlatformAccount platform)
    {
        var steam = SteamConnectService.Current;
        var path = platform.ExePath;
        var target = account as SteamAccount;
        var fromTray = target is not null && trayAccounts.TryGetValue(target.SteamUser, out var rowAccount)
            && ReferenceEquals(target, rowAccount);

        bool CanSwitchTrayAccount()
        {
            if (target is null || target.SteamUser.SteamId64 == GetConfirmedAccountId(steam)
                || string.IsNullOrWhiteSpace(target.AccountName))
                return false;

            var cached = steam.SteamUsers.Lookup(target.SteamUser.SteamId64);
            return cached.HasValue && ReferenceEquals(cached.Value, target.SteamUser);
        }

        if (fromTray && !CanSwitchTrayAccount())
            return false;

        await KillPlatformProcess(platform);
        if (!string.Equals(path, platform.ExePath, StringComparison.Ordinal) || (fromTray && !CanSwitchTrayAccount()))
            return false;

        var users = fromTray
            ? steam.SteamUsers.Items.ToArray()
            : platform.Accounts?.Cast<SteamAccount>().Select(s => s.SteamUser).ToArray();
        if (users.Any_Nullable())
        {
            if (account is SteamAccount steamAccount && !string.IsNullOrEmpty(steamAccount?.AccountName))
            {
                await steamService.SetSteamCurrentUserAsync(steamAccount.AccountName);
                foreach (var user in users)
                {
                    if (user.AccountName == steamAccount.AccountName)
                    {
                        user.AutoLogin = true;
                        user.MostRecent = true;
                        user.WantsOfflineMode = steamAccount.WantsOfflineMode;
                        user.SkipOfflineModeWarning = steamAccount.SkipOfflineModeWarning;

                        if (steamAccount.PersonaState != PersonaState.Default)
                            ISteamService.Instance.SetPersonaState(steamAccount.SteamUser.SteamId32.ToString(),
                                steamAccount.PersonaState);
                    }
                    else
                    {
                        user.MostRecent = false;
                        user.AutoLogin = false;
                    }

                    var optional = SteamConnectService.Current.SteamUsers.Lookup(user.SteamId64);
                    if (optional.HasValue)
                    {
                        optional.Value.MostRecent = user.MostRecent;
                        optional.Value.AutoLogin = user.AutoLogin;
                    }
                }
            }
            else
            {
                await ClearCurrentLoginUser(platform);
                foreach (var user in users)
                {
                    user.MostRecent = false;
                    user.AutoLogin = false;
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
        if (Interlocked.CompareExchange(ref switching, 1, 0) != 0)
            return;

        try
        {
            await ClearCurrentLoginUser(platform);
            await SwapToAccountCore(null, platform);
        }
        finally
        {
            Volatile.Write(ref switching, 0);
        }
    }

    public ValueTask<bool> CurrnetUserAdd(string name, PlatformAccount platform) => ValueTask.FromResult(false);

    public string GetCurrentAccountId(PlatformAccount platform) =>
        steamService.GetRememberUserList()?.FirstOrDefault(static user => user.MostRecent)?.SteamId64.ToString() ?? string.Empty;

    public Task<IEnumerable<IAccount>?> GetUsers(PlatformAccount platform, Action? refreshUsers = null)
    {
        SteamConnectService.Current.RefreshSteamUsers(requireReadSuccess: true);
        Task2.InBackground(async () =>
        {
            await SteamConnectService.Current.RefreshSteamUsersInfo();
            refreshUsers?.Invoke();
        });

        var users = SteamConnectService.Current.SteamUsers.Items;
        return Task.FromResult<IEnumerable<IAccount>?>(users.Select(static user => new SteamAccount(user)));
    }

    public bool SetPlatformPath(PlatformAccount platform)
    {
        return false;
    }

    public async Task<bool> DeleteAccountInfo(IAccount account, PlatformAccount platform)
    {
        if (account is SteamAccount steamAccount)
        {
            var path = platform.ExePath;
            var result =
                await MessageBox.ShowAsync(Strings.UserChange_DeleteUserTip, button: MessageBox.Button.OKCancel);
            if (result == MessageBox.Result.OK)
            {
                result = await MessageBox.ShowAsync(Strings.UserChange_DeleteUserDataTip,
                    button: MessageBox.Button.OKCancel);
                if (!string.Equals(path, platform.ExePath, StringComparison.Ordinal))
                    return false;

                if (result == MessageBox.Result.OK)
                {
                    steamService.DeleteLocalUserData(steamAccount.SteamUser, true);
                }
                else
                {
                    steamService.DeleteLocalUserData(steamAccount.SteamUser, false);
                }

                platform.Accounts?.Remove(account);
                var id = steamAccount.SteamUser.SteamId64;
                if (steamService.GetRememberUserList()?.Any(user => user.SteamId64 == id) != true
                    && string.Equals(path, platform.ExePath, StringComparison.Ordinal))
                    SteamConnectService.Current.SteamUsers.RemoveKey(id);
                return true;
            }
        }

        return false;
    }
}