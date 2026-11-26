using System.Text.Json;
using BD.WTTS.GameList.Categories;

namespace BD.WTTS.UnitTest;

[TestFixture]
public sealed class CategoryStoreParserTest
{
    [Test]
    public void ReadsRegionalPricesAndFeaturesWithoutErasingCachedTags()
    {
        using var response = JsonDocument.Parse("""
            { "620": { "success": true, "data": {
                "price_overview": { "currency": "CNY", "initial": 4200, "final": 2100, "discount_percent": 50 },
                "is_free": false, "release_date": { "date": "18 Apr, 2011" },
                "genres": [{ "description": "Action" }], "supported_languages": "English<strong>*</strong>, Simplified Chinese<br>* Full audio",
                "categories": [{ "id": 2 }, { "id": 38 }], "platforms": { "windows": true, "linux": true }
            }}}
            """);
        Assert.That(CategoryStoreParser.TryDetails(response.RootElement, 620, out var data), Is.True);
        var facts = CategoryStoreParser.ReadDetails(data, new() { Tags = ["Puzzle"] });

        Assert.Multiple(() =>
        {
            Assert.That(facts.Price, Is.EqualTo(21m));
            Assert.That(facts.OriginalPrice, Is.EqualTo(42m));
            Assert.That(facts.Discount, Is.EqualTo(50));
            Assert.That(facts.Currency, Is.EqualTo("CNY"));
            Assert.That(facts.ReleaseYear, Is.EqualTo(2011));
            Assert.That(facts.Languages, Is.EqualTo(new[] { "English", "Simplified Chinese" }));
            Assert.That(facts.Platforms, Is.EqualTo(new[] { "windows", "linux" }));
            Assert.That(facts.Tags, Is.EqualTo(new[] { "Puzzle" }));
            Assert.That(facts.Singleplayer, Is.True);
            Assert.That(facts.Multiplayer, Is.True);
        });
    }

    [Test]
    public void DistinguishesUnlistedPricesFromFreeGames()
    {
        using var unlisted = JsonDocument.Parse("""{"is_free":false}""");
        using var free = JsonDocument.Parse("""{"is_free":true}""");
        Assert.That(CategoryStoreParser.ReadDetails(unlisted.RootElement, new()).Price, Is.Null);
        Assert.That(CategoryStoreParser.ReadDetails(free.RootElement, new()).Price, Is.Zero);
    }

    [Test]
    public void ReadsCurrentReviewSummaryAndPublicCommunityTags()
    {
        using var response = JsonDocument.Parse("""
            {"response":{"query_summary":{"review_score":8,"total_positive":92,"total_reviews":100}}}
            """);
        var facts = CategoryStoreParser.ReadReviews(response.RootElement, new());
        Assert.That(facts?.ReviewScore, Is.EqualTo(92m));
        var page = """
            <script> InitAppTagModal(620, [{"name":"Puzzle [Logic]"},{"name":"Short"},{"name":"SHORT"}], []); </script>
            """;
        var store = CategoryStoreParser.ReadStorePage(page, facts!);
        Assert.That(store?.Tags, Is.EqualTo(new[] { "Puzzle [Logic]", "Short" }));
        Assert.That(store?.ReviewTier, Is.EqualTo(8));
    }

    [Test]
    public void ReadsDeckCompatibilityFromPlatformBatch()
    {
        using var response = JsonDocument.Parse("""
            { "response": { "store_items": [
                { "appid": 620, "platforms": { "windows": true, "steam_deck_compat_category": 3 } },
                { "appid": 440, "platforms": { "windows": true, "steam_deck_compat_category": 1 } },
                { "appid": 730, "platforms": { "windows": true } }
            ] } }
            """);
        Assert.That(CategoryStoreParser.ReadPlatforms(response.RootElement),
            Is.EqualTo(new (uint, string?)[] { (620, "verified"), (440, "unsupported"), (730, null) }));
    }
}
