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

    public string? DisplayName
    {
        get => this.SteamUser.SteamNickName;
        set => this.SteamUser.SteamID = value;
    }

    public string? AccountId
    {
        get => this.SteamUser.SteamId64.ToString();
        set => this.SteamUser.SteamId64 = Convert.ToInt64(value);
    }

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
        get => this.SteamUser.AvatarMedium;
        set => this.SteamUser.AvatarMedium = value;
    }

    public ThirdpartyPlatform Platform { get; init; }

    public string? PlatformName { get; init; }

}
