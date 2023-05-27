namespace BD.WTTS.Models;

public interface IAccount
{
    string? DisplayName { get; set; }

    string? AccountId { get; set; }

    string? AccountName { get; set; }

    DateTime? LastLoginTime { get; set; }

    string? ImagePath { get; set; }

    ThirdpartyPlatform Platform { get; init; }

    string? PlatformName { get; init; }
}
