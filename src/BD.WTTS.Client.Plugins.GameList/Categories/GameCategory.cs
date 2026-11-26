namespace BD.WTTS.GameList.Categories;

internal enum CategoryMode { Manual, Auto, Mixed }

internal enum CategoryRuleKind { Condition, All, Any, Not }

internal enum CategoryOperator
{
    Equal, NotEqual, GreaterThan, AtLeast, LessThan, AtMost, Between,
    Contains, In, HasTag, HasAny, HasAll, IsEmpty, WithinDays, OlderThanDays,
}

internal sealed record CategoryRule(
    CategoryRuleKind Kind, string Field, CategoryOperator Operator, string[] Values, CategoryRule[] Children)
{
    public static CategoryRule Condition(string field, CategoryOperator op, params string[] values) =>
        new(CategoryRuleKind.Condition, field, op, values, []);

    public static CategoryRule Group(CategoryRuleKind kind, params CategoryRule[] children) =>
        new(kind, string.Empty, default, [], children);
}

internal sealed record GameCategory(string Name, CategoryMode Mode, CategoryRule Rules)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public uint[] ManualIncluded { get; init; } = [];
    public uint[] ManualExcluded { get; init; } = [];
}

internal sealed record CategoryGame(uint AppId, string Name)
{
    public decimal? PlaytimeMinutes { get; init; }
    public decimal? RecentPlaytimeMinutes { get; init; }
    public DateTimeOffset? LastPlayed { get; init; }
    public DateTimeOffset? OwnedSince { get; init; }
    public bool? Installed { get; init; }
    public long? SizeOnDisk { get; init; }
    public bool? Hidden { get; init; }
    public bool? Favourite { get; init; }
    public decimal? AchievementCompletion { get; init; }
    public int? AchievementCount { get; init; }
    public bool? FamilyShared { get; init; }
    public string? Type { get; init; }
    public string[]? Platforms { get; init; }
    public int? ReleaseYear { get; init; }
    public string? Developer { get; init; }
    public string? Publisher { get; init; }
    public string? DeckStatus { get; init; }
    public CategoryStoreFacts? Store { get; init; }
}

internal sealed record CategoryStoreFacts
{
    public decimal? Price { get; init; }
    public decimal? OriginalPrice { get; init; }
    public decimal? Discount { get; init; }
    public string? Currency { get; init; }
    public bool? Free { get; init; }
    public int? ReleaseYear { get; init; }
    public string[]? Genres { get; init; }
    public string[]? Tags { get; init; }
    public string[]? Developers { get; init; }
    public string[]? Publishers { get; init; }
    public decimal? ReviewScore { get; init; }
    public int? ReviewCount { get; init; }
    public int? ReviewTier { get; init; }
    public int? Metacritic { get; init; }
    public int? AchievementCount { get; init; }
    public string? DeckStatus { get; init; }
    public string? Controller { get; init; }
    public string[]? Languages { get; init; }
    public bool? Singleplayer { get; init; }
    public bool? Multiplayer { get; init; }
    public string[]? Platforms { get; init; }
    public string? Type { get; init; }
}
