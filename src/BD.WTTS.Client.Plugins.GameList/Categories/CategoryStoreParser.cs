using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BD.WTTS.GameList.Categories;

internal static partial class CategoryStoreParser
{
    public static bool TryDetails(JsonElement root, uint id, out JsonElement data)
    {
        var item = Property(root, id.ToString(CultureInfo.InvariantCulture));
        data = Property(item, "data");
        return Flag(item, "success") == true && data.ValueKind == JsonValueKind.Object;
    }

    public static CategoryStoreFacts ReadDetails(JsonElement data, CategoryStoreFacts previous)
    {
        var categories = Array(data, "categories")?.Select(item => Integer(item, "id")).ToHashSet();
        var platforms = Property(data, "platforms");
        var languages = ReadString(data, "supported_languages");
        var release = ReadString(Property(data, "release_date"), "date");
        var year = release is null ? Match.Empty : YearRegex().Match(release);
        var free = Flag(data, "is_free");
        var result = previous with
        {
            Free = free,
            ReleaseYear = year.Success ? int.Parse(year.Value, CultureInfo.InvariantCulture) : null,
            Genres = Names(data, "genres"),
            Developers = Strings(data, "developers"),
            Publishers = Strings(data, "publishers"),
            Metacritic = Integer(Property(data, "metacritic"), "score"),
            AchievementCount = Integer(Property(data, "achievements"), "total"),
            Controller = ReadString(data, "controller_support") ?? (categories is null ? null :
                categories.Contains(28) ? "full" : categories.Contains(18) ? "partial" : "none"),
            Languages = languages is null ? null : HtmlBreakRegex().Split(languages)[0].Split(',')
                .Select(item => WebUtility.HtmlDecode(HtmlTagRegex().Replace(item, string.Empty)).Trim(' ', '*'))
                .Where(item => item.Length != 0).ToArray(),
            Singleplayer = categories?.Contains(2),
            Multiplayer = categories?.Overlaps([1, 9, 20, 24, 27, 36, 37, 38, 39]),
            Platforms = platforms.ValueKind != JsonValueKind.Object ? null :
                new[] { "windows", "mac", "linux" }.Where(platform => Flag(platforms, platform) == true).ToArray(),
            Type = ReadString(data, "type"),
        };
        return ReadPrice(data, result);
    }

    public static CategoryStoreFacts ReadPrice(JsonElement data, CategoryStoreFacts previous)
    {
        var price = Property(data, "price_overview");
        var currency = ReadString(price, "currency");
        // Valve's price_overview amounts are in hundredths, including zero-decimal storefront currencies.
        return previous with
        {
            Price = currency != null ? Integer(price, "final") / 100m : previous.Free == true ? 0 : null,
            OriginalPrice = currency != null ? Integer(price, "initial") / 100m : previous.Free == true ? 0 : null,
            Discount = currency != null ? Integer(price, "discount_percent") : previous.Free == true ? 0 : null,
            Currency = currency,
        };
    }

    public static CategoryStoreFacts? ReadReviews(JsonElement root, CategoryStoreFacts previous)
    {
        var summary = Property(Property(root, "response"), "query_summary");
        if (summary.ValueKind == JsonValueKind.Undefined)
            summary = Property(root, "query_summary");
        if (Integer(summary, "total_reviews") is not { } count || count < 0)
            return null;
        var positive = Integer(summary, "total_positive");
        return previous with
        {
            ReviewCount = count,
            ReviewScore = count > 0 && positive is >= 0 && positive <= count ? positive * 100m / count : null,
            ReviewTier = Integer(summary, "review_score"),
        };
    }

    public static CategoryStoreFacts? ReadStorePage(string page, CategoryStoreFacts previous)
    {
        var modal = TagModalRegex().Match(page);
        string[]? tags = null;
        if (modal.Success)
        {
            // Parse the exact JSON array and let the JSON reader handle brackets inside quoted tag names.
            var bytes = Encoding.UTF8.GetBytes(page.AsSpan(modal.Index + modal.Length - 1).ToString());
            var reader = new Utf8JsonReader(bytes);
            using var data = JsonDocument.ParseValue(ref reader);
            tags = data.RootElement.EnumerateArray().Select(item => ReadString(item, "name"))
                .OfType<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }
        else
        {
            var links = TagLinkRegex().Matches(page);
            if (links.Count != 0)
                tags = links.Select(match => WebUtility.HtmlDecode(match.Groups[1].Value).Trim())
                    .Where(tag => tag.Length != 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        return tags is null ? null : previous with { Tags = tags };
    }

    public static IEnumerable<(uint Id, string? Deck)> ReadPlatforms(JsonElement root)
    {
        var items = Array(Property(root, "response"), "store_items");
        if (items is null)
            yield break;
        foreach (var item in items.Value)
        {
            if (Integer(item, "appid") is not { } id || id <= 0)
                continue;
            var platforms = Property(item, "platforms");
            if (platforms.ValueKind != JsonValueKind.Object)
                continue;
            yield return ((uint)id, Integer(platforms, "steam_deck_compat_category") switch
            {
                3 => "verified",
                2 => "playable",
                1 => "unsupported",
                0 => "untested",
                _ => null,
            });
        }
    }

    static JsonElement Property(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) ? value : default;

    static string? ReadString(JsonElement element, string name) => Text(Property(element, name));

    static string? Text(JsonElement value) => value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    static int? Integer(JsonElement element, string name)
    {
        var value = Property(element, name);
        return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var result) ? result :
            int.TryParse(Text(value), NumberStyles.None, CultureInfo.InvariantCulture, out result) ? result : null;
    }

    static bool? Flag(JsonElement element, string name) => Property(element, name).ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null,
    };

    static JsonElement.ArrayEnumerator? Array(JsonElement element, string name)
    {
        var value = Property(element, name);
        return value.ValueKind == JsonValueKind.Array ? value.EnumerateArray() : null;
    }

    static string[]? Names(JsonElement data, string name) =>
        Array(data, name)?.Select(item => ReadString(item, "description")).OfType<string>().ToArray();

    static string[]? Strings(JsonElement data, string name) => Array(data, name)?.Select(Text).OfType<string>().ToArray();

    [GeneratedRegex(@"\b(?:19|20)\d{2}\b")]
    private static partial Regex YearRegex();

    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlBreakRegex();

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"InitAppTagModal\s*\(\s*\d+\s*,\s*\[")]
    private static partial Regex TagModalRegex();

    [GeneratedRegex("""<a\b[^>]*\bclass=["']app_tag["'][^>]*>([^<]+)</a>""", RegexOptions.IgnoreCase)]
    private static partial Regex TagLinkRegex();

}
