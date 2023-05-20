namespace BD.WTTS.Models;

public interface IAccount
{
    string DisplayName { get; set; }

    string? AccountId { get; set; }

    string? AccountName { get; set; }

    string? PlatformId { get; set; }

    DateTime? LastLoginTime { get; set; }

    AuthenticatorPlatform Platform { get; set; }

    string? PlatformName { get; set; }

    string? ImagePath { get; set; }
}
