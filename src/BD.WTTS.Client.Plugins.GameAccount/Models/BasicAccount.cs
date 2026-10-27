namespace BD.WTTS.Models;

public sealed class BasicAccount : ReactiveObject, IAccount
{
    public BasicAccount(string id)
    {
        AccountId = id;
    }

    public string? DisplayName => AccountName;

    [Reactive]
    public string? AliasName { get; set; }

    public string? AccountName { get; set; }

    public string AccountId { get; set; }

    [Reactive]
    public DateTime? LastLoginTime { get; set; }

    [Reactive]
    public string? ImagePath { get; set; }

    public string? AvatarFramePath { get; set; }

    public ThirdpartyPlatform Platform { get; init; }

    public string? PlatformName { get; init; }

    bool mostRecent;

    public bool MostRecent
    {
        get => mostRecent;
        set
        {
            this.RaiseAndSetIfChanged(ref mostRecent, value);
            this.RaisePropertyChanged(nameof(IsRecentLogin));
        }
    }

    public bool IsRecentLogin => MostRecent;

    public bool RememberPassword => false;

    public string? AvatarMedium { get; set; }
}
