namespace BD.WTTS.Models;

public interface IAccount : IReactiveObject
{
    string? DisplayName { get; }

    string? AliasName { get; set; }

    string AccountId { get; set; }

    string? AccountName { get; set; }

    DateTime? LastLoginTime { get; set; }

    bool MostRecent { get; set; }

    string? ImagePath { get; set; }

    string? AvatarFramePath { get; set; }

    ThirdpartyPlatform Platform { get; init; }

    string? PlatformName { get; init; }

    string? AvatarMedium { get; set; }
}
