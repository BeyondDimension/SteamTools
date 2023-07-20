namespace BD.WTTS.Models;

public sealed class BasicAccount : ReactiveObject, IAccount
{
    public string? DisplayName { get; set; }

    public string? AccountName { get; set; }

    public string? AccountId { get; set; }

    public DateTime? LastLoginTime { get; set; }

    public string? ImagePath { get; set; }

    public ThirdpartyPlatform Platform { get; init; }

    public string? PlatformName { get; init; }

    public bool MostRecent { get; set; }
}
