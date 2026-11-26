using static BD.WTTS.GameList.Categories.CategoryOperator;

namespace BD.WTTS.GameList.Categories;

internal enum CategoryValueKind { Number, Boolean, Date, Text, Choice, List, Tags }

internal sealed record CategoryField(
    string Id, CategoryValueKind Kind, Func<CategoryGame, object?> Read, string[]? Choices = null)
{
    static readonly CategoryOperator[] NumberOperators = [Equal, NotEqual, GreaterThan, AtLeast, LessThan, AtMost, Between];
    static readonly CategoryOperator[] BooleanOperators = [Equal, NotEqual];
    static readonly CategoryOperator[] DateOperators = [Equal, NotEqual, GreaterThan, AtLeast, LessThan, AtMost, Between, WithinDays, OlderThanDays];
    static readonly CategoryOperator[] TextOperators = [Equal, NotEqual, CategoryOperator.Contains, In, IsEmpty];
    static readonly CategoryOperator[] ListOperators = [CategoryOperator.Contains, HasAny, HasAll, IsEmpty];
    static readonly CategoryOperator[] TagOperators = [HasTag, HasAny, HasAll, IsEmpty];
    static readonly CategoryOperator[] ChoiceOperators = [Equal, NotEqual, In];

    public IReadOnlyList<CategoryOperator> Operators => Kind switch
    {
        CategoryValueKind.Number => NumberOperators,
        CategoryValueKind.Boolean => BooleanOperators,
        CategoryValueKind.Date => DateOperators,
        CategoryValueKind.Choice => ChoiceOperators,
        CategoryValueKind.List => ListOperators,
        CategoryValueKind.Tags => TagOperators,
        _ => TextOperators,
    };

    public static IReadOnlyDictionary<string, CategoryField> Registry { get; } = new CategoryField[]
    {
        new("PlaytimeMinutes", CategoryValueKind.Number, g => g.PlaytimeMinutes),
        new("RecentPlaytimeMinutes", CategoryValueKind.Number, g => g.RecentPlaytimeMinutes),
        new("LastPlayed", CategoryValueKind.Date, g => g.LastPlayed),
        new("OwnedSince", CategoryValueKind.Date, g => g.OwnedSince),
        new("Installed", CategoryValueKind.Boolean, g => g.Installed),
        new("SizeOnDisk", CategoryValueKind.Number, g => g.SizeOnDisk is { } size ? size / 1_073_741_824m : null),
        new("Hidden", CategoryValueKind.Boolean, g => g.Hidden),
        new("Favourite", CategoryValueKind.Boolean, g => g.Favourite),
        new("AchievementCompletion", CategoryValueKind.Number, g => g.AchievementCompletion),
        new("AchievementCount", CategoryValueKind.Number, g => (decimal?)(g.AchievementCount ?? g.Store?.AchievementCount)),
        new("FamilyShared", CategoryValueKind.Boolean, g => g.FamilyShared),
        new("Type", CategoryValueKind.Choice, g => NormalizeType(g.Type ?? g.Store?.Type), ["game", "dlc", "application", "tool", "demo", "other"]),
        new("Platforms", CategoryValueKind.List, g => g.Platforms ?? g.Store?.Platforms, ["windows", "mac", "linux"]),
        new("Price", CategoryValueKind.Number, g => g.Store?.Price),
        new("OriginalPrice", CategoryValueKind.Number, g => g.Store?.OriginalPrice),
        new("Discount", CategoryValueKind.Number, g => g.Store?.Discount),
        new("PricePerHour", CategoryValueKind.Number, PricePerHour),
        new("ValueRating", CategoryValueKind.Number, ValueRating),
        new("Free", CategoryValueKind.Boolean, g => g.Store?.Free),
        new("ReleaseYear", CategoryValueKind.Number, g => (decimal?)(g.ReleaseYear ?? g.Store?.ReleaseYear)),
        new("Genres", CategoryValueKind.List, g => g.Store?.Genres),
        new("Tags", CategoryValueKind.Tags, g => g.Store?.Tags),
        new("TagCount", CategoryValueKind.Number, g => (decimal?)g.Store?.Tags?.Length),
        new("Developer", CategoryValueKind.List, g => g.Store?.Developers ?? Single(g.Developer)),
        new("Publisher", CategoryValueKind.List, g => g.Store?.Publishers ?? Single(g.Publisher)),
        new("ReviewScore", CategoryValueKind.Number, g => g.Store?.ReviewScore),
        new("ReviewCount", CategoryValueKind.Number, g => (decimal?)g.Store?.ReviewCount),
        new("ReviewTier", CategoryValueKind.Choice, g => g.Store?.ReviewTier?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["9", "8", "7", "6", "5", "4", "3", "2", "1", "0"]),
        new("Metacritic", CategoryValueKind.Number, g => (decimal?)g.Store?.Metacritic),
        new("DeckStatus", CategoryValueKind.Choice, g => g.DeckStatus ?? g.Store?.DeckStatus,
            ["verified", "playable", "unsupported", "untested"]),
        new("Controller", CategoryValueKind.Choice, g => g.Store?.Controller, ["full", "partial", "none"]),
        new("Languages", CategoryValueKind.List, g => g.Store?.Languages),
        new("Singleplayer", CategoryValueKind.Boolean, g => g.Store?.Singleplayer),
        new("Multiplayer", CategoryValueKind.Boolean, g => g.Store?.Multiplayer),
    }.ToDictionary(field => field.Id, StringComparer.Ordinal);

    static string[]? Single(string? value) => value is null ? null : [value];

    static string? NormalizeType(string? type) => type?.ToLowerInvariant() switch
    {
        null or "" => null,
        "software" or "application" => "application",
        "game" or "dlc" or "tool" or "demo" => type.ToLowerInvariant(),
        _ => "other",
    };

    static object? PricePerHour(CategoryGame game) =>
        game.Store?.Price is { } price && game.PlaytimeMinutes is { } minutes
            ? price / Math.Max(minutes / 60m, 0.1m)
            : null;

    // 0–100: all-language positive review percentage divided by (1 + current regional price per played hour).
    // Free games with a known playtime keep their review percentage. Compare within one regional currency.
    static object? ValueRating(CategoryGame game) =>
        game.Store?.ReviewScore is { } score && PricePerHour(game) is decimal cost
            ? score / (1 + cost)
            : null;
}
