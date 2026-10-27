namespace BD.WTTS.Models;

public sealed class SteamAccount : ReactiveObject, IAccount
{
    public SteamAccount(SteamUser user)
    {
        Platform = ThirdpartyPlatform.Steam;
        PlatformName = nameof(ThirdpartyPlatform.Steam);
        SteamUser = user;
        var weakAccount = new WeakReference<SteamAccount>(this);
        System.ComponentModel.PropertyChangedEventHandler? handler = null;
        handler = (sender, args) =>
        {
            if (!weakAccount.TryGetTarget(out var account))
            {
                if (sender is System.ComponentModel.INotifyPropertyChanged source)
                    source.PropertyChanged -= handler;
                return;
            }

            if (args.PropertyName == nameof(SteamUser.AutoLogin))
            {
                account.RaisePropertyChanged(nameof(AutoLogin));
                account.RaisePropertyChanged(nameof(IsRecentLogin));
            }
            else if (args.PropertyName == nameof(SteamUser.RememberPassword))
                account.RaisePropertyChanged(nameof(RememberPassword));
            else if (args.PropertyName == nameof(SteamUser.MostRecent))
                account.RaisePropertyChanged(nameof(MostRecent));
        };
        user.PropertyChanged += handler;
    }

    public SteamUser SteamUser { get; }

    public string? DisplayName => SteamUser.SteamNickName;

    public string AccountId
    {
        get => SteamUser.SteamId64.ToString();
        set => SteamUser.SteamId64 = Convert.ToInt64(value);
    }

    public string? AliasName
    {
        get => SteamUser.Remark;
        set
        {
            SteamUser.Remark = value;
            this.RaisePropertyChanged();
        }
    }

    public string? AccountName
    {
        get => SteamUser.AccountName;
        set => SteamUser.AccountName = value;
    }

    public DateTime? LastLoginTime
    {
        get => SteamUser.LastLoginTime;
        set => SteamUser.LastLoginTime = value ?? DateTime.MinValue;
    }

    public string? ImagePath
    {
        get => SteamUser.MiniProfile?.AnimatedAvatar ?? SteamUser.AvatarMedium;
        set => SteamUser.AvatarMedium = value;
    }

    public string? AvatarFramePath
    {
        get => SteamUser.MiniProfile?.AvatarFrame;
        set => SteamUser.MiniProfile.AvatarFrame = value;
    }

    public bool MostRecent
    {
        get => SteamUser.MostRecent;
        set => SteamUser.MostRecent = value;
    }

    public bool AutoLogin
    {
        get => SteamUser.AutoLogin;
        set => SteamUser.AutoLogin = value;
    }

    public bool IsRecentLogin => AutoLogin;

    public bool WantsOfflineMode
    {
        get => SteamUser.WantsOfflineMode;
        set => SteamUser.WantsOfflineMode = value;
    }

    public bool SkipOfflineModeWarning
    {
        get => SteamUser.SkipOfflineModeWarning;
        set => SteamUser.SkipOfflineModeWarning = value;
    }

    public bool RememberPassword
    {
        get => SteamUser.RememberPassword;
        set => SteamUser.RememberPassword = value;
    }

    public PersonaState PersonaState
    {
        get => SteamUser.PersonaState;
        set => SteamUser.PersonaState = value;
    }

    public ThirdpartyPlatform Platform { get; init; }

    public string? PlatformName { get; init; }

    public string? AvatarMedium
    {
        get => SteamUser.AvatarMedium;
        set => SteamUser.AvatarMedium = value;
    }
}
