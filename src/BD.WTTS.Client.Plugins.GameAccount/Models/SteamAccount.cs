namespace BD.WTTS.Models;

public sealed class SteamAccount : ReactiveObject, IAccount
{
    public SteamAccount(SteamUser user)
    {
        this.Platform = ThirdpartyPlatform.Steam;
        this.PlatformName = nameof(ThirdpartyPlatform.Steam);
        this.SteamUser = user;
    }

    public SteamUser SteamUser { get; init; }

    public string? DisplayName => this.SteamUser.SteamNickName;

    public string? AccountId
    {
        get => this.SteamUser.SteamId64.ToString();
        set => this.SteamUser.SteamId64 = Convert.ToInt64(value);
    }

    [Reactive]
    public string? AliasName { get; set; }

    public string? AccountName
    {
        get => this.SteamUser.AccountName;
        set => this.SteamUser.AccountName = value;
    }

    public DateTime? LastLoginTime
    {
        get => this.SteamUser.LastLoginTime;
        set => this.SteamUser.LastLoginTime = value ?? DateTime.MinValue;
    }

    public string? ImagePath
    {
        get => this.SteamUser.MiniProfile?.AnimatedAvatar ?? this.SteamUser.AvatarMedium;
        set => this.SteamUser.AvatarMedium = value;
    }

    public bool MostRecent
    {
        get => this.SteamUser.MostRecent;
        set => this.SteamUser.MostRecent = value;
    }

    public bool WantsOfflineMode
    {
        get => this.SteamUser.WantsOfflineMode;
        set => this.SteamUser.WantsOfflineMode = value;
    }

    public bool SkipOfflineModeWarning
    {
        get => this.SteamUser.SkipOfflineModeWarning;
        set => this.SteamUser.SkipOfflineModeWarning = value;
    }

    public bool RememberPassword
    {
        get => this.SteamUser.RememberPassword;
        set => this.SteamUser.RememberPassword = value;
    }

    public PersonaState PersonaState
    {
        get => this.SteamUser.PersonaState;
        set => this.SteamUser.PersonaState = value;
    }

    public ThirdpartyPlatform Platform { get; init; }

    public string? PlatformName { get; init; }

}
