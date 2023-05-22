namespace BD.WTTS.Models;

public sealed class Account : IAccount
{
    public string DisplayName { get; set; } = string.Empty;

    public string? AccountId { get; set; }

    public string? AccountName { get; set; }

    public string? PlatformId { get; set; }

    public DateTime? LastLoginTime { get; set; }

    public GamePlatform Platform { get; set; }

    public string? PlatformName { get; set; }

    public string? ImagePath { get; set; }
}
